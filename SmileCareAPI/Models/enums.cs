namespace SmileCareAPI.Models
{
    public enum UserRole
    {
        Admin = 1,
        Doctor = 2,
        Receptionist = 3,
        Patient = 4
    }

    public enum AppointmentStatus
    {
        Pending = 1,
        Confirmed = 2,
        Completed = 3,
        Cancelled = 4,
        NoShow = 5
    }

    public enum AppointmentType
    {
        FirstConsultation = 1,
        FollowUp = 2,
        Treatment = 3
    }

    public enum DiagnosisType
    {
        Healthy = 1,
        Cavity = 2,
        GumDisease = 3
    }

    public enum SeverityLevel
    {
        Mild = 1,
        Moderate = 2,
        Severe = 3
    }

    public enum TreatmentStatus
    {
        Planned = 1,
        Ongoing = 2,
        Completed = 3,
        OnHold = 4,
        Cancelled = 5
    }

    public enum ImageType
    {
        IntraoralPhoto = 1,
        XRay = 2,
        Scan = 3,
        Other = 4
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum PrescriptionStatus
    {
        Active = 1,
        Completed = 2,
        Expired = 3
    }

    public enum DocumentType
    {
        Prescription = 1,
        LabReport = 2,
        ConsentForm = 3,
        Insurance = 4,
        Other = 5
    }

    public enum DiagnosisStatus
    {
        Active = 1,
        Resolved = 2,
        Chronic = 3
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Partial = 2,
        Paid = 3,
        Refunded = 4
    }

    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        BankTransfer = 3,
        Insurance = 4,
        PaymentPlan = 5,
        MobilePayment = 6
    }
}
