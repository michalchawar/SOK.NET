using SOK.Domain.Entities.Parish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOK.Application.Common.DTO.Submitter
{
    public class SubmitterDto
    {
        public int Id { get; set; }
        public string UniqueId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public SubmitterDto(SOK.Domain.Entities.Parish.Submitter submitter)
        {
            Id = submitter.Id;
            UniqueId = submitter.UniqueId.ToString();
            Name = submitter.Name;
            Surname = submitter.Surname;
            Email = submitter.Email ?? string.Empty;
            Phone = submitter.Phone ?? string.Empty;
        }
    }
}
