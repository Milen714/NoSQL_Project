using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.Models
{
    public enum FranchiseLocation
    {
		//Noord Holland
		[Display(Name = "Amsterdam Centrum")] AmsterdamCentrum,
		[Display(Name = "Amsterdam Zuid")] AmsterdamZuid,
		[Display(Name = "Alkmaar Centrum")] AlkmaarCentrum,
		[Display(Name = "Haarlem Centrum")] HaarlemCentrum,

		//South Holland
		[Display(Name = "Rotterdam Centrum")] RotterdamCentrum,
		[Display(Name = "Rotterdam Alexander")] RotterdamAlexander,
		[Display(Name = "Den Haag Centrum")] DenHaagCentrum,
		[Display(Name = "Delft Centrum")] DelftCentrum,

		//Utrecht
		[Display(Name = "Utrecht Centrum")] UtrechtCentrum,
		[Display(Name = "Leidsche Rijn")] LeidscheRijn,
		[Display(Name = "Amersfoort Centrum")] AmersfoortCentrum,

		//Groningen
		[Display(Name = "Groningen Centrum")] GroningenCentrum,

		//Friesland
		[Display(Name = "Leeuwarden Centrum")] LeeuwardenCentrum,

		//Gelderland
		[Display(Name = "Apeldoorn Centrum")] ApeldoornCentrum,
		[Display(Name = "Nijmegen Centrum")] NijmegenCentrum,
		[Display(Name = "Arnhem Centrum")] ArnhemCentrum,

		//Limburg
		[Display(Name = "Maastricht Centrum")] MaastrichtCentrum,
		[Display(Name = "Heerlen Centrum")] HeerlenCentrum,		

		//North Brabant
		[Display(Name = "Eindhoven Centrum")] EindhovenCentrum,
		[Display(Name = "Tilburg Centrum")] TilburgCentrum,
		[Display(Name = "Breda Centrum")] BredaCentrum,
		[Display(Name = "Den Bosch")] DenBosch,

		//Overijssel
		[Display(Name = "Zwolle Centrum")] ZwolleCentrum,
		[Display(Name = "Enschede Centrum")] EnschedeCentrum,

		//Flevoland
		[Display(Name = "Almere Centrum")] AlmereCentrum,

	}
}
