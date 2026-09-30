namespace TrainTracker.App.Models;

// نفس شكل TripCardDto اللي بيرجعه /api/trips/today
public record TripCard(int TripId, string TrainNumber, string? TrainType,
    string? From, string? To, TimeOnly? Departure, TimeOnly? Arrival,
    string Status, int DelayMinutes, double? LastLatitude, double? LastLongitude);

public record Station(int Id, string NameAr, string? NameEn, double Latitude, double Longitude);

public record TripSearchResult(int TripId, string TrainNumber, string? TrainType,
    TimeOnly? Departure, TimeOnly? Arrival, int StopsCount, string Status, int DelayMinutes);

public record TripStop(int StationId, string StationName, double Latitude, double Longitude,
    int Order, TimeOnly? ScheduledArrival, TimeOnly? ScheduledDeparture, int DayOffset);

public record TripDetails(int TripId, string TrainNumber, string? TrainType, string Status,
    int DelayMinutes, double? LastLatitude, double? LastLongitude, List<TripStop> Stops);

public record PositionReport(double Latitude, double Longitude, double? SpeedKmh, Guid? UserId);
