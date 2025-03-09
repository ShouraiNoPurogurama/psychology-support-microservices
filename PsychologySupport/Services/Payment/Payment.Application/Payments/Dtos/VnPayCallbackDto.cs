using Microsoft.AspNetCore.Mvc;

namespace Payment.Application.Payments.Dtos;

public record VnPayCallbackDto
{
    [FromQuery(Name = "vnp_TransactionStatus")]
    public string? TransactionStatus { get; set; }

    [FromQuery(Name = "vnp_TransactionNo")]
    public string? TransactionNo { get; set; }

    [FromQuery(Name = "vnp_TxnRef")]
    public string? TransactionCode { get; set; }

    [FromQuery(Name = "vnp_ResponseCode")]
    public string? ResponseCode { get; set; }

    [FromQuery(Name = "vnp_OrderInfo")]
    public string? OrderInfo { get; set; }

    [FromQuery(Name = "vnp_Amount")]
    public float Amount { get; set; }

    public bool Success => "00".Equals(ResponseCode);
}