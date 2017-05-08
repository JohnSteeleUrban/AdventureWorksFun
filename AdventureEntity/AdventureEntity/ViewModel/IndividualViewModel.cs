using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using AdventureEntity.Models;

namespace AdventureEntity.ViewModel
{
    public class IndividualViewModel
    {
        [Key]
        public int Key { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [StringLength(128)]
        public string Password { get; set; }
        [StringLength(25)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [Required]
        [StringLength(50)]
        public string CardType { get; set; }
        [Required]
        [StringLength(25)]
        public string CardNumber { get; set; }
        [Required]
        public byte ExpMonth { get; set; }
        [Required]
        public short ExpYear { get; set; }



    }
}