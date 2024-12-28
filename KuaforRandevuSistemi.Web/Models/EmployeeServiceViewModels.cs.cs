namespace KuaforRandevuSistemi.Web.Models
{
    public class EmployeeServicesViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public List<ServiceCheckBoxViewModel> Services { get; set; }
    }

    public class ServiceCheckBoxViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public bool IsSelected { get; set; }
    }
}