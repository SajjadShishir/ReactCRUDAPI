using Microsoft.AspNetCore.Mvc;

namespace ReactEvidenceApi.Models.Dto
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime AdmissionDate { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? image { get; set; }

        public string AddressesJson { get; set; }
        
    }

    public class AddressDTO
    {
        public string Street { get; set; }
        public string City { get; set; }
    
    }
}
