using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReactEvidenceApi.Models;
using ReactEvidenceApi.Models.Dto;
using static System.Net.Mime.MediaTypeNames;

namespace ReactEvidenceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentsController(AppDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<ActionResult<List<Student>>> Get()
        {
            return await _db.Students.Include(pc => pc.Addresses).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> Get(int id)
        {
            var student = await _db.Students.Include(x => x.Addresses).FirstOrDefaultAsync(x => x.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            return student;
        }





        [HttpPost]

        public async Task<ActionResult<StudentDto>> Post([FromForm] StudentDto student)
        {
            if (student.image != null)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(student.image.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await student.image.CopyToAsync(fileStream);
                    }

                    student.ImageUrl = $"/uploads/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error uploading image", error = ex.Message });
                }
            }

            List<AddressDTO> addresses = new List<AddressDTO>();
            if (!string.IsNullOrWhiteSpace(student.AddressesJson))
            {
                addresses = JsonConvert.DeserializeObject<List<AddressDTO>>(student.AddressesJson);
            }

            var addStu = new Student
            {
                Name = student.Name,
                AdmissionDate = student.AdmissionDate,
                IsActive = student.IsActive,
                ImageUrl = student.ImageUrl
            };

            _db.Students.Add(addStu);
            await _db.SaveChangesAsync();

            var newStu = _db.Students.FirstOrDefault(x => x.ImageUrl == student.ImageUrl);

            addStu.Addresses = addresses.Select(a => new Address
            {
                Street = a.Street,
                City = a.City,
                StudentId = newStu.Id
            }).ToList();

            await _db.SaveChangesAsync();

            return Ok(addStu);
        }








        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] StudentDto studentDto)
        {
            //if (id != studentDto.Id)
            //{
            //    return BadRequest("Student ID mismatch.");
            //}

        
            var existingStudent = await _db.Students.Include(s => s.Addresses)
                                                     .FirstOrDefaultAsync(s => s.Id == id);
            if (existingStudent == null)
            {
                return NotFound("Student not found.");
            }

         
            existingStudent.Name = studentDto.Name;
            existingStudent.AdmissionDate = studentDto.AdmissionDate;
            existingStudent.IsActive = studentDto.IsActive;

            
            if (studentDto.image != null)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                   
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(studentDto.image.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await studentDto.image.CopyToAsync(fileStream);
                    }

                 
                    existingStudent.ImageUrl = $"/uploads/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error uploading image", error = ex.Message });
                }
            }

           
            List<Address> addresses = new List<Address>();
            if (!string.IsNullOrWhiteSpace(studentDto.AddressesJson))
            {
                addresses = JsonConvert.DeserializeObject<List<Address>>(studentDto.AddressesJson);
            }

        
            var addressIds = addresses.Select(a => a.Id).ToList();

        
            foreach (var address in addresses)
            {
                if (address.Id != 0)
                {
                    
                    var existingAddress = existingStudent.Addresses.FirstOrDefault(a => a.Id == address.Id);
                    if (existingAddress != null)
                    {
                        existingAddress.Street = address.Street;
                        existingAddress.City = address.City;
                    }
                }
                else
                {
                    
                    var newAddress = new Address
                    {
                        Street = address.Street,
                        City = address.City,
                        StudentId = existingStudent.Id
                    };
                    _db.Addresses.Add(newAddress);
                }
            }

           
            var addressesToDelete = existingStudent.Addresses.Where(a => !addressIds.Contains(a.Id)).ToList();
            _db.RemoveRange(addressesToDelete);

            await _db.SaveChangesAsync();

            return Ok();
        }




        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var student = await _db.Students.Include(s => s.Addresses).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null)
            {
                return NotFound();
            }
            _db.Addresses.RemoveRange(student.Addresses);
            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
