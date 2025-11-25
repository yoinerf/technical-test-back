using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace InvestmentFunds.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IAmazonSimpleEmailService sesClient,
        IAmazonSimpleNotificationService snsClient,
        ICustomerRepository customerRepository,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _sesClient = sesClient;
        _snsClient = snsClient;
        _customerRepository = customerRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendSubscriptionNotificationAsync(
        string customerId,
        string fundName,
        decimal amount,
        NotificationPreference preference)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            _logger.LogWarning("No se pudo enviar notificación: Cliente {CustomerId} no encontrado", customerId);
            return;
        }

        var subject = "Suscripción Exitosa a Fondo de Inversión";
        var message = $"Se ha suscrito exitosamente al fondo {fundName} por un monto de ${amount:N0} COP.";

        await SendNotificationAsync(customer, subject, message, preference, "suscripción");
    }

    public async Task SendCancellationNotificationAsync(
        string customerId,
        string fundName,
        decimal refundAmount,
        NotificationPreference preference)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            _logger.LogWarning("No se pudo enviar notificación: Cliente {CustomerId} no encontrado", customerId);
            return;
        }

        var subject = "Cancelación de Suscripción a Fondo";
        var message = $"Su suscripción al fondo {fundName} ha sido cancelada. " +
                     $"Se ha reembolsado ${refundAmount:N0} COP a su saldo disponible.";

        await SendNotificationAsync(customer, subject, message, preference, "cancelación");
    }

    private async Task SendNotificationAsync(
        Customer customer,
        string subject,
        string message,
        NotificationPreference preference,
        string eventType)
    {
        var emailSuccess = false;
        var smsSuccess = false;

        try
        {
            if (preference == NotificationPreference.Email || preference == NotificationPreference.Both)
            {
                emailSuccess = await SendEmailAsync(customer.Email, subject, message);
            }

            if (preference == NotificationPreference.SMS || preference == NotificationPreference.Both)
            {
                smsSuccess = await SendSmsAsync(customer.Phone, message);
            }

            if ((preference == NotificationPreference.Email && emailSuccess) ||
                (preference == NotificationPreference.SMS && smsSuccess) ||
                (preference == NotificationPreference.Both && (emailSuccess || smsSuccess)))
            {
                _logger.LogInformation(
                    "Notificación de {EventType} enviada exitosamente a {CustomerId} (Email: {EmailSuccess}, SMS: {SmsSuccess})",
                    eventType, customer.CustomerId, emailSuccess, smsSuccess);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error general enviando notificación de {EventType} para {CustomerId}", 
                eventType, customer.CustomerId);
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        // Validar email
        if (string.IsNullOrWhiteSpace(toEmail) || !IsValidEmail(toEmail))
        {
            _logger.LogWarning("Email inválido o vacío: {Email}", toEmail);
            return false;
        }

        var fromEmail = _configuration["AWS:SES:FromEmail"];
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            _logger.LogError("AWS:SES:FromEmail no está configurado");
            return false;
        }

        try
        {
            var request = new SendEmailRequest
            {
                Source = fromEmail,
                Destination = new Destination { ToAddresses = new List<string> { toEmail } },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Text = new Content(body),
                        Html = new Content($@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                    <h2 style='color: #333;'>{subject}</h2>
                                    <p style='color: #666; line-height: 1.6;'>{body}</p>
                                    <hr style='border: 1px solid #eee; margin: 20px 0;'>
                                    <p style='color: #999; font-size: 12px;'>
                                        Este es un mensaje automático de Investment Funds Platform.
                                    </p>
                                </div>
                            </body>
                            </html>")
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(request);
            _logger.LogInformation("Email enviado exitosamente a {Email} (MessageId: {MessageId})", toEmail, response.MessageId);
            return true;
        }
        catch (MessageRejectedException ex)
        {
            _logger.LogError(ex, 
                "Email rechazado por SES para {Email}. Verifica que el email esté verificado en AWS SES Console.", 
                toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error enviando email a {Email}. Verifica configuración de AWS SES y credenciales.", 
                toEmail);
            return false;
        }
    }

    private async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Número de teléfono vacío o nulo");
            return false;
        }

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning(
                "Número de teléfono inválido: {Phone}. Debe estar en formato E.164 (+573001234567)", 
                phoneNumber);
            return false;
        }

        try
        {
            var request = new PublishRequest
            {
                PhoneNumber = phoneNumber,
                Message = message,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "AWS.SNS.SMS.SMSType",
                        new MessageAttributeValue 
                        { 
                            DataType = "String", 
                            StringValue = "Transactional" 
                        }
                    }
                }
            };

            var response = await _snsClient.PublishAsync(request);
            _logger.LogInformation(
                "SMS enviado exitosamente a {Phone} (MessageId: {MessageId})", phoneNumber, response.MessageId);
            return true;
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error enviando SMS a {Phone}. Verificar configuración de AWS SNS, límites de gasto y credenciales.",
                phoneNumber);
            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}