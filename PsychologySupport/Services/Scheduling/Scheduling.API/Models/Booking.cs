﻿using Scheduling.API.Enums;

namespace Scheduling.API.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public DateOnly Date { get; set; }  
        public TimeOnly StartTime { get; set; }  
        public int Duration { get; set; } 
        public decimal Price { get; set; }
        public Guid? PromoCodeId { get; set; }
        public Guid? GiftCodeId { get; set; }
        public BookingStatus Status { get; set; }
    }
}
