<Query Kind="Program">
  <Connection>
    <ID>eac52b14-3979-444b-a13b-e212aaecf64f</ID>
    <Server>localhost\sqlexpress</Server>
    <Database>gzDevDb</Database>
    <ShowServer>true</ShowServer>
  </Connection>
</Query>

void Main()
{
	
	
	var portfolioDtos = (from p in Portfolios
						  // older implementation
						 //join c in CustPortfolios on p.Id equals c.PortfolioId
						 //join b in InvBalances on
							// new { CustomerId = c.CustomerId, YearMonth = c.YearMonth } equals
							// new { CustomerId = b.CustomerId, YearMonth = b.YearMonth }
							//where c.CustomerId == 8
						 join b in InvBalances on p.Id equals b.PortfolioId
						 where b.CustomerId == 8
							   && !b.Sold
						 group b by p
		into g
						 select new PortfolioDto
						 {
							 Id = g.Key.Id,
							 Title = g.Key.Title,
							 Color = g.Key.Color,
							 ROI = g.Key.PortfolioPortFunds.Select(f => f.Weight * f.Fund.YearToDate / 100).Sum(),
							 Risk = (RiskToleranceEnum)g.Key.RiskTolerance,
							 AllocatedAmount = g.Sum(b => b.CashInvestment),
							 Holdings = g.Key.PortfolioPortFunds.Select(f => new HoldingDto
							 {
								 Name = f.Fund.HoldingName,
								 Weight = f.Weight
							 })
						 }).AsEnumerable()
		/*** Union with non-allocated customer portfolio ****/
		.Union(
			(from p in Portfolios
			 where p.IsActive
			 select new PortfolioDto
			 {
				 Id = p.Id,
				 Title = p.Title,
				 Color = p.Color,
				 ROI = p.PortfolioPortFunds.Select(f => f.Weight * f.Fund.YearToDate / 100).Sum(),
				 Risk = (RiskToleranceEnum)p.RiskTolerance,
				 AllocatedAmount = 0M,
				 Holdings = p.PortfolioPortFunds.Select(f => new HoldingDto
				 {
					 Name = f.Fund.HoldingName,
					 Weight = f.Weight
				 })
			 }), new PortfolioComparer())
		.ToList();

	var totalSum = portfolioDtos.Sum(p => p.AllocatedAmount);
	foreach (var portfolioDto in portfolioDtos)
	{
		if (totalSum != 0)
		{
			portfolioDto.AllocatedPercent = (float)(100 * portfolioDto.AllocatedAmount / totalSum);
		}
		portfolioDto.Selected = portfolioDto.Id == 3;
	}

	portfolioDtos.Dump();

}

// Define other methods and classes here
public class PortfolioDto
{
	public int Id { get; set; }
	public string Title { get; set; }
	public double ROI { get; set; }
	public string Color { get; set; }
	public bool Selected { get; set; }
	public RiskToleranceEnum Risk { get; set; }
	public float AllocatedPercent { get; set; }
	public decimal AllocatedAmount { get; set; }
	public IEnumerable<HoldingDto> Holdings { get; set; }
}

public class HoldingDto
{
	public string Name { get; set; }
	public double Weight { get; set; }
}

public enum RiskToleranceEnum
{

	Low = 1,

	Low_Medium,

	Medium,

	Medium_High,

	High
}

class PortfolioComparer : IEqualityComparer<PortfolioDto>
{
	public bool Equals(PortfolioDto p1, PortfolioDto p2)
	{
		return p1.Id == p2.Id;
	}

	public int GetHashCode(PortfolioDto p)
	{
		return p.Id.GetHashCode();
	}
}