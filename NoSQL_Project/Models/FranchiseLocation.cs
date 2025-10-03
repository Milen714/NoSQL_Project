using System.ComponentModel.DataAnnotations;

namespace NoSQL_Project.Models
{
    public enum FranchiseLocation
    {
		// North Holland
		[Display(Name = "Amsterdam Centrum")] AmsterdamCityCenter,
		[Display(Name = "Amsterdam Zuidas")] AmsterdamZuidas,
		[Display(Name = "Amsterdam Schiphol Airport")] AmsterdamSchipholAirport,
		[Display(Name = "Haarlem Grote Markt")] HaarlemGroteMarkt,
		[Display(Name = "Alkmaar Cheese Market")] AlkmaarCheeseMarket,
		[Display(Name = "Hoofddorp Station Square")] HoofddorpStationSquare,
		[Display(Name = "Zaandam City Center")] ZaandamCityCenter,

		// South Holland
		[Display(Name = "Rotterdam Central Station")] RotterdamCentralStation,
		[Display(Name = "Rotterdam Kop van Zuid")] RotterdamKopVanZuid,
		[Display(Name = "Rotterdam Alexandrium")] RotterdamAlexandrium,
		[Display(Name = "The Hague City Center")] TheHagueCityCenter,
		[Display(Name = "The Hague Scheveningen")] TheHagueScheveningen,
		[Display(Name = "Leiden Central")] LeidenCentral,
		[Display(Name = "Delft Markt Square")] DelftMarktSquare,
		[Display(Name = "Dordrecht Harbor")] DordrechtHarbor,

		// Utrecht
		[Display(Name = "Utrecht Hoog Catharijne")] UtrechtHoogCatharijne,
		[Display(Name = "Utrecht Neude Square")] UtrechtNeudeSquare,
		[Display(Name = "Amersfoort City Center")] AmersfoortCityCenter,
		[Display(Name = "Veenendaal Corridor")] VeenendaalCorridor,
		[Display(Name = "Nieuwegein City Plaza")] NieuwegeinCityPlaza,

		// Flevoland
		[Display(Name = "Almere City Center")] AlmereCityCenter,
		[Display(Name = "Almere Stad")] AlmereStad,
		[Display(Name = "Lelystad Batavia Stad")] LelystadBataviaStad,
		[Display(Name = "Dronten City Center")] DrontenCityCenter,
		[Display(Name = "Zeewolde Waterfront")] ZeewoldeWaterfront
	}
}
