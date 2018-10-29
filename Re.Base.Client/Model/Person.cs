using System;
using System.Collections.Generic;
using System.Text;

namespace Re.Base.Client.Model
{

	public class Friend
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class Name
	{
		public string First { get; set; }
		public string Last { get; set; }
	}

	public class Person
	{
		public bool IsActive {get; set;}
		public string Balance { get; set; }
		public string Picture { get; set; }
		public int Age { get; set; }
		public string EyeColor { get; set; }
		public string Name { get; set; }
		public string Company { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Address { get; set; }
		public string About { get; set; }
		public string Registered { get; set; }
		public decimal Latitude { get; set; }
		public decimal Longitude { get; set; }
		//public string[] Tags { get; set; }
		//public int[] Range { get; set; }
        
		public string Greeting { get; set; }
		public string FavoriteFruit { get; set; }

    }
}
