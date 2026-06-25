namespace EventosVivos_Api.Util.Const
{
    /// <summary>
    /// Constantes de reglas de negocio para el módulo de reservas.
    /// </summary>
    public static class ReservationBusinessConstants
    {
        /// <summary>
        /// Cantidad mínima permitida para una reserva.
        /// </summary>
        public const int MinReservationQuantity = 1;

        /// <summary>
        /// Límite de cantidad de entradas para eventos costosos.
        /// </summary>
        public const int MaxReservationQuantityForExpensiveEvents = 10;

        /// <summary>
        /// Límite de cantidad de entradas para eventos urgentes (menos de 24 horas para iniciar).
        /// </summary>
        public const int MaxReservationQuantityForUrgentEvents = 5;

        /// <summary>
        /// Umbral de precio que define a un evento costoso.
        /// </summary>
        public const float ExpensiveEventPriceThreshold = 100f;

        /// <summary>
        /// Cantidad de horas restante para definir que un evento es urgente.
        /// </summary>
        public const int UrgentEventHoursThreshold = 24;

        /// <summary>
        /// Cantidad mínima de horas previas al inicio de un evento en las que está permitido reservar.
        /// </summary>
        public const int MinimumHoursBeforeEventToReserve = 1;

        /// <summary>
        /// Cantidad de horas previas al inicio del evento límite para cancelar sin penalización.
        /// </summary>
        public const int HoursLimitBeforeEventToLoseReservation = 48;
    }
}
