using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Vylersant_Facturacion.Contracts.Customers;
using Vylersant_Facturacion.Contracts.Products;

namespace Vylersant_Facturacion.Desktop.PointOfSale;

public sealed class PosViewModel(ICatalogApiClient catalogClient) : INotifyPropertyChanged
{
    private readonly List<ProductResponse> _allProducts = [];
    private string _searchText = string.Empty;
    private bool _isLoading;
    private string? _errorMessage;
    private CustomerResponse? _selectedCustomer;

    public ObservableCollection<ProductResponse> Products { get; } = [];
    public ObservableCollection<CustomerResponse> Customers { get; } = [];
    public ObservableCollection<CartLineViewModel> Cart { get; } = [];

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;
            _searchText = value;
            OnPropertyChanged();
            ApplyProductFilter();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoProducts)); }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set { _errorMessage = value; OnPropertyChanged(); }
    }

    public CustomerResponse? SelectedCustomer
    {
        get => _selectedCustomer;
        set { _selectedCustomer = value; OnPropertyChanged(); }
    }

    public decimal Subtotal => decimal.Round(Cart.Sum(line => line.LineTotal), 2, MidpointRounding.AwayFromZero);
    public bool HasNoProducts => !IsLoading && string.IsNullOrWhiteSpace(ErrorMessage) && Products.Count == 0;

    public async Task LoadAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var productsTask = catalogClient.GetProductsAsync(accessToken, cancellationToken);
            var customersTask = catalogClient.GetCustomersAsync(accessToken, cancellationToken);
            await Task.WhenAll(productsTask, customersTask);

            _allProducts.Clear();
            _allProducts.AddRange((await productsTask).Where(product => product.IsActive));
            Customers.Clear();
            foreach (var customer in (await customersTask).Where(customer => customer.IsActive).OrderBy(customer => customer.Name))
                Customers.Add(customer);
            if (SelectedCustomer is not null && Customers.All(customer => customer.Id != SelectedCustomer.Id))
                SelectedCustomer = null;
            ApplyProductFilter();
        }
        catch (CatalogApiException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "No se pudo conectar con el servidor. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            ErrorMessage = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void AddProduct(ProductResponse product)
    {
        if (!product.IsActive)
            return;

        var existing = Cart.FirstOrDefault(line => line.ProductId == product.Id);
        if (existing is null)
        {
            Cart.Add(new CartLineViewModel(product));
            Cart[^1].PropertyChanged += CartLine_PropertyChanged;
        }
        else
        {
            existing.Quantity += 1m;
        }
        OnCartChanged();
    }

    public void ChangeQuantity(CartLineViewModel line, decimal delta)
    {
        if (!Cart.Contains(line))
            return;
        var next = line.Quantity + delta;
        if (next < 1m)
            Cart.Remove(line);
        else
            line.Quantity = next;
        OnCartChanged();
    }

    public void RemoveProduct(CartLineViewModel line)
    {
        if (Cart.Remove(line))
            OnCartChanged();
    }

    public void Clear()
    {
        Cart.Clear();
        Customers.Clear();
        Products.Clear();
        _allProducts.Clear();
        SelectedCustomer = null;
        OnCartChanged();
    }

    private void ApplyProductFilter()
    {
        Products.Clear();
        var query = SearchText.Trim();
        foreach (var product in _allProducts.Where(product =>
                     query.Length == 0 || product.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                     (product.Sku?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false)))
            Products.Add(product);
        OnPropertyChanged(nameof(HasNoProducts));
    }

    private void CartLine_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CartLineViewModel.Quantity) or nameof(CartLineViewModel.LineTotal))
            OnCartChanged();
    }

    private void OnCartChanged()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(HasCartItems));
    }

    public bool HasCartItems => Cart.Count > 0;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

