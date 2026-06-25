namespace EventosVivos_Api.Util.Const
{
    public static class MessageConstants
    {
        public const string FutureStartDateError = "La fecha y hora de inicio deben ser en el futuro.";
        public const string EndDateBeforeStartDateError = "La fecha y hora de fin deben ser posteriores a la fecha y hora de inicio.";
        public const string WeekendStartTimeError = "Los eventos en sábado o domingo no pueden comenzar después de las 22:00.";
        public const string VenueNotFoundError = "El venue especificado no existe.";
        public const string CapacityExceededError = "La capacidad máxima del evento no puede superar la capacidad del venue.";
        public const string EventOverlapError = "Ya existe un evento en el mismo venue que se superpone con las fechas y horas especificadas.";
        public const string InvalidEventTypeError = "El tipo de evento especificado no existe.";
        public const string EventNotFoundError = "El evento especificado no existe.";
        public const string InvalidEmailFormatError = "El formato del correo electrónico del comprador no es válido.";
        public const string EventStartTooSoonError = "No se puede reservar para eventos que inicien en menos de 1 hora.";
        public const string NotEnoughTicketsError = "La cantidad de entradas solicitadas supera las disponibles.";
        public const string MinQuantityError = "La cantidad mínima de entradas a reservar es 1.";
        public const string MaxQuantityPriceLimitError = "La cantidad máxima de entradas a reservar para este evento es 10.";
        public const string MaxQuantityTimeLimitError = "En este momento para este evento la cantidad máxima es de 5.";
        public const string ReservaNotFoundError = "La reserva especificada no existe.";
        public const string OnlyConfirmedReservationsCanBeCancelledError = "Solo se pueden cancelar reservas en estado CONFIRMADA.";
        public const string ReservationAlreadyConfirmedError = "La reserva ya está confirmada.";
        public const string ReservationAlreadyCancelledError = "La reserva está cancelada.";
    }
}
