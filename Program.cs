using Microsoft.EntityFrameworkCore;
using static Trade;

Trade trade = Trade.CreateNew();
trade.AddOrder(1m, 55000m);

class Order
{
    public readonly record struct OrderId(Guid Value)
    {
        public static OrderId Empty = new(Guid.Empty);
        public static OrderId CreateNew() => new(Guid.NewGuid());
    }

    public OrderId Id { get; init; } = OrderId.Empty;
    public TradeId TradeId { get; init; } = TradeId.Empty;
    public PositiveDecimal Quantity { get; init; }
    public PositiveDecimal Price { get; init; }
    public DateTime DateCreated { get; init; }

    public virtual Trade? Trade { get; set; }

    public static Order CreateNew(TradeId tradeId, PositiveDecimal quantity, PositiveDecimal price) => new()
    {
        Id = OrderId.CreateNew(),
        TradeId = tradeId,
        Quantity = quantity,
        Price = price,
    };

    public override string ToString()
    {
        return $"Order(Id = {Id.Value}, Quantity = {Quantity.Value:N2}, Price = {Price.Value:N2})";
    }
}

readonly record struct PositiveDecimal(decimal Value)
{
    public decimal Value { get; init; } =
        Value >= 0m ? Value
        : throw new ArgumentException("Value must be positive.");

    public static PositiveDecimal CreateNew(decimal value) => new(value);
    public static implicit operator decimal(PositiveDecimal value) => value.Value;
    public static implicit operator PositiveDecimal(decimal value) => new(value);
}

class Trade
{
    public readonly record struct TradeId(Guid Value)
    {
        public static TradeId Empty = new(Guid.Empty);
        public static TradeId CreateNew() => new(Guid.NewGuid());
    }

    public TradeId Id { get; init; } = TradeId.Empty;
    public List<Order> Orders { get; init; } = [];

    public static Trade CreateNew() => new()
    {
        Id = TradeId.CreateNew()
    };

    public void AddOrder(PositiveDecimal quantity, PositiveDecimal price)
    {
        var order = Order.CreateNew(Id, quantity, price);
        Orders.Add(order);
    }
}

class TradeDbContext : DbContext
{
    public DbSet<Trade> Trades { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Catalog=trade-db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trade>()
            .Property(trade => trade.Id)
            .HasConversion(id => id.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .Property(order => order.Id)
            .HasConversion(id => id.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .Property(order => order.TradeId)
            .HasConversion(id => id.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .Property(order => order.Quantity)
            .HasConversion(quantity => quantity.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .Property(order => order.Price)
            .HasConversion(price => price.Value, value => new(value));

        modelBuilder.Entity<Order>()
            .HasOne(t => t.Trade)
            .WithMany(o => o.Orders);
    }
}
