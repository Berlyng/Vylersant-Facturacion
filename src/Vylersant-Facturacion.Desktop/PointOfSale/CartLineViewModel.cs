using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vylersant_Facturacion.Contracts.Products;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public sealed class CartLineViewModel(ProductResponse product) : INotifyPropertyChanged
{
    private decimal _quantity = 1m;

    public ProductResponse Product { get; } = product;
    public Guid ProductId => Product.Id;
    public string Name => Product.Name;
    public decimal UnitPrice => Product.Price;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (value < 1m || value == _quantity)
                return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => decimal.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

