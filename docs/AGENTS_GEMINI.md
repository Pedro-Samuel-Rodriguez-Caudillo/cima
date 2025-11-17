# ?? GEMINI: GUÍA ESPECIALIZADA - Frontend & Blazor

**Rol:** Frontend Engineer especializado en Blazor, UX, CSS, Accesibilidad  
**Responsabilidad:** Componentes Razor, páginas interactivas, estilos, UX  
**Stack:** Blazor Web App (.NET 9), Blazorise, Tailwind CSS, HTML/CSS  
**Commits:** `feat(blazor)`, `feat(blazor-client)`, `style`, `a11y`, `fix`

---

## ?? TU MISIÓN

Eres el **maestro de la experiencia del usuario**. Tu código es:
- ? Hermoso (diseño limpio, responsive)
- ? Accesible (ARIA, labels, contrast)
- ? Rápido (lazy loading, optimizado)
- ? Amigable (UX intuitiva, errores claros)

**No toques:** Lógica C# (validaciones, permisos), DevOps (Docker), persistencia.

---

## ?? ÁREA DE TRABAJO

```
src/cima.Blazor/
??? Pages/                               ? TU ESPACIO ??
?   ??? Properties/
?   ?   ??? Index.razor (catálogo)
?   ?   ??? Detail.razor (detalle)
?   ?   ??? Index.razor.cs (interactividad)
?   ??? Architects/
?   ?   ??? Index.razor (listado)
?   ?   ??? Profile.razor (perfil)
?   ??? Admin/
?   ?   ??? Properties/
?   ?   ??? ContactRequests/
?   ?   ??? Statistics/
?   ??? Home.razor
?
??? Shared/                              ? TU ESPACIO (Componentes)
?   ??? PropertyCard.razor
?   ??? PropertyImageGallery.razor
?   ??? ContactForm.razor
?   ??? AdminLayout.razor
?   ??? PublicLayout.razor
?
??? wwwroot/                             ? TU ESPACIO
    ??? css/
    ?   ??? app.css (global + Tailwind)
    ?   ??? custom.css
    ??? js/
        ??? app.js (JS interop)
        ??? carousel.js (libs)

src/cima.Blazor.Client/
??? (Componentes WASM interactivos)      ? TU ESPACIO ??
```

---

## ?? FLUJO: Cómo Pensar (Frontend)

### **1. Feature Request**
```
"Necesito página de catálogo de propiedades
con filtros, paginación y cards responsivas"
```

### **2. Tu Análisis (UX)**
```
Componentes necesarios:
  - PropertyList (página contenedor)
  - PropertyCard (componente reutilizable)
  - PropertyImageGallery (galería)
  - FilterPanel (filtros, mobile drawer)
  - Pagination (paginación)

Rutas:
  - /properties (SSR, público, sin auth)
  - /properties/{id} (detalle)

Estilos:
  - Mobile: 1 columna, stack vertical
  - Tablet: 2 columnas
  - Desktop: 3 columnas

Accesibilidad:
  - Labels para filtros
  - Alt text en imágenes
  - Botones con ARIA labels
  - Navegación por teclado (Tab)
```

### **3. Código**

#### **Paso 1: Componente Contenedor (Página)**

```razor
@* Pages/Properties/Index.razor *@
@page "/properties"
@using cima.Blazor.Pages.Properties
@using cima.Domain.Shared.Dtos
@inject IPropertyAppService PropertyAppService
@inject MessageService MessageService

<PageTitle>Catálogo de Propiedades | CIMA</PageTitle>

<div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white shadow-sm sticky top-0 z-10">
        <div class="container mx-auto px-4 py-6">
            <h1 class="text-4xl font-bold text-gray-900">
                Catálogo de Propiedades
            </h1>
            <p class="text-gray-600 mt-2">
                Descubre nuestras propiedades inmobiliarias disponibles
            </p>
        </div>
    </header>

    <!-- Content -->
    <main class="container mx-auto px-4 py-8">
        <div class="grid grid-cols-1 lg:grid-cols-4 gap-8">
            
            <!-- Filters Sidebar (Desktop) -->
            <aside class="hidden lg:block">
                <PropertyFilterPanel 
                    @ref="FilterPanel"
                    OnFilterChanged="HandleFilterChangedAsync"
                />
            </aside>

            <!-- Mobile Filter Toggle -->
            <div class="lg:hidden mb-4">
                <button 
                    @onclick="ToggleMobileFilters"
                    class="w-full bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
                    aria-label="Abrir filtros">
                    <i class="fas fa-filter mr-2"></i> Filtros
                </button>
            </div>

            <!-- Mobile Filters (Drawer) -->
            @if (ShowMobileFilters)
            {
                <div class="fixed inset-0 z-40 bg-black/50 lg:hidden" @onclick="ToggleMobileFilters"></div>
                <div class="fixed left-0 top-0 z-50 h-full w-64 bg-white overflow-y-auto lg:hidden">
                    <button 
                        @onclick="ToggleMobileFilters"
                        class="absolute top-4 right-4 p-2 text-gray-600 hover:text-gray-900"
                        aria-label="Cerrar filtros">
                        <i class="fas fa-times text-xl"></i>
                    </button>
                    <div class="p-6">
                        <PropertyFilterPanel 
                            @ref="FilterPanel"
                            OnFilterChanged="HandleFilterChangedAsync"
                        />
                    </div>
                </div>
            }

            <!-- Results -->
            <div class="lg:col-span-3">
                
                <!-- Loading State -->
                @if (Loading)
                {
                    <div class="flex items-center justify-center py-12">
                        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                    </div>
                }
                
                <!-- Error State -->
                @if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    <div class="bg-red-50 border-l-4 border-red-400 p-4 mb-6">
                        <div class="flex">
                            <i class="fas fa-exclamation-circle text-red-400 mt-0.5 mr-3"></i>
                            <div>
                                <p class="text-red-800 font-medium">Oops, ocurrió un error</p>
                                <p class="text-red-700 text-sm mt-1">@ErrorMessage</p>
                            </div>
                        </div>
                    </div>
                }
                
                <!-- Empty State -->
                @if (!Loading && (Properties == null || Properties.Count == 0) && string.IsNullOrEmpty(ErrorMessage))
                {
                    <div class="text-center py-12">
                        <i class="fas fa-inbox text-6xl text-gray-300 mb-4"></i>
                        <p class="text-gray-600 text-lg">No se encontraron propiedades</p>
                        <p class="text-gray-500 mt-2">Intenta con otros filtros</p>
                    </div>
                }
                
                <!-- Properties Grid -->
                @if (Properties?.Count > 0)
                {
                    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        @foreach (var property in Properties)
                        {
                            <PropertyCard Property="property" />
                        }
                    </div>

                    <!-- Pagination -->
                    <nav class="flex items-center justify-center gap-2 mt-8" aria-label="Paginación">
                        <button 
                            @onclick="PreviousPageAsync"
                            disabled="@(CurrentPage <= 1)"
                            class="px-3 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            aria-label="Página anterior">
                            <i class="fas fa-chevron-left"></i>
                        </button>

                        @for (int i = 1; i <= TotalPages; i++)
                        {
                            int pageNum = i;
                            <button 
                                @onclick="() => GoToPageAsync(pageNum)"
                                class="@($"px-3 py-2 rounded-lg {(CurrentPage == pageNum ? "bg-blue-600 text-white" : "border border-gray-300 hover:bg-gray-50")}")"
                                aria-label="Ir a página @i"
                                aria-current="@(CurrentPage == pageNum ? "page" : null)">
                                @i
                            </button>
                        }

                        <button 
                            @onclick="NextPageAsync"
                            disabled="@(CurrentPage >= TotalPages)"
                            class="px-3 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                            aria-label="Página siguiente">
                            <i class="fas fa-chevron-right"></i>
                        </button>
                    </nav>
                }
            </div>
        </div>
    </main>
</div>

@code {
    private List<PropertyDto>? Properties;
    private PropertyFilterPanel? FilterPanel;
    private bool Loading = true;
    private bool ShowMobileFilters = false;
    private string? ErrorMessage;

    // Pagination
    private int CurrentPage = 1;
    private int TotalPages = 1;
    private const int PageSize = 12;

    protected override async Task OnInitializedAsync()
    {
        await LoadPropertiesAsync();
    }

    private async Task LoadPropertiesAsync()
    {
        try
        {
            Loading = true;
            ErrorMessage = null;

            var skipCount = (CurrentPage - 1) * PageSize;
            var filter = FilterPanel?.GetFilter() ?? new PropertyFilterDto();

            var result = await PropertyAppService.GetPublishedAsync(
                filter,
                skipCount,
                PageSize
            );

            Properties = result.Items?.ToList();
            TotalPages = (int)Math.Ceiling((double)result.TotalCount / PageSize);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Logger.LogError(ex, "Error loading properties");
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task HandleFilterChangedAsync()
    {
        CurrentPage = 1;
        ToggleMobileFilters();
        await LoadPropertiesAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadPropertiesAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadPropertiesAsync();
        }
    }

    private async Task GoToPageAsync(int pageNum)
    {
        CurrentPage = pageNum;
        await LoadPropertiesAsync();
    }

    private void ToggleMobileFilters()
    {
        ShowMobileFilters = !ShowMobileFilters;
    }
}
```

#### **Paso 2: Componente PropertyCard (Reutilizable)**

```razor
@* Shared/PropertyCard.razor *@
@namespace cima.Blazor.Shared
@using cima.Domain.Shared.Dtos

<article class="bg-white rounded-lg shadow-md hover:shadow-xl transition-shadow duration-300">
    
    <!-- Image Container -->
    <div class="relative overflow-hidden rounded-t-lg bg-gray-200 h-64">
        @if (Property?.Images?.Any() == true)
        {
            <img 
                src="@Property.Images.First().Url" 
                alt="@Property.Images.First().AltText"
                class="w-full h-full object-cover hover:scale-105 transition-transform duration-300"
                loading="lazy"
            />
        }
        else
        {
            <div class="w-full h-full flex items-center justify-center">
                <i class="fas fa-image text-4xl text-gray-400"></i>
            </div>
        }

        <!-- Badge -->
        <div class="absolute top-4 right-4">
            <span class="inline-block px-3 py-1 bg-blue-600 text-white text-sm font-semibold rounded-full">
                Destacada
            </span>
        </div>
    </div>

    <!-- Content -->
    <div class="p-4">
        
        <!-- Title -->
        <h3 class="text-lg font-bold text-gray-900 truncate mb-2">
            @Property?.Title
        </h3>

        <!-- Location -->
        <p class="text-sm text-gray-600 flex items-center mb-4">
            <i class="fas fa-map-marker-alt mr-2 text-red-500"></i>
            @Property?.Location
        </p>

        <!-- Features Grid -->
        <div class="grid grid-cols-3 gap-2 mb-4 text-sm">
            <div class="text-center p-2 bg-gray-50 rounded">
                <i class="fas fa-door-open text-blue-600 block mb-1"></i>
                <span class="text-gray-700 font-semibold">@Property?.Bedrooms</span>
                <p class="text-xs text-gray-500">Recámaras</p>
            </div>
            <div class="text-center p-2 bg-gray-50 rounded">
                <i class="fas fa-bath text-blue-600 block mb-1"></i>
                <span class="text-gray-700 font-semibold">@Property?.Bathrooms</span>
                <p class="text-xs text-gray-500">Baños</p>
            </div>
            <div class="text-center p-2 bg-gray-50 rounded">
                <i class="fas fa-ruler-combined text-blue-600 block mb-1"></i>
                <span class="text-gray-700 font-semibold">@Property?.Area m²</span>
                <p class="text-xs text-gray-500">Área</p>
            </div>
        </div>

        <!-- Price -->
        <div class="mb-4 pb-4 border-b border-gray-200">
            <p class="text-2xl font-bold text-green-600">
                $@Property?.Price.ToString("N0")
            </p>
        </div>

        <!-- Architect Info -->
        @if (Property?.Architect != null)
        {
            <div class="flex items-center gap-3 mb-4">
                <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                    <i class="fas fa-user text-blue-600"></i>
                </div>
                <div class="flex-1">
                    <p class="text-sm font-semibold text-gray-900">
                        @Property.Architect.UserName
                    </p>
                </div>
            </div>
        }

        <!-- Actions -->
        <div class="flex gap-2">
            <a 
                href="/properties/@Property?.Id" 
                class="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors text-center font-medium"
                aria-label="Ver detalle de @Property?.Title">
                Ver Detalle
            </a>
        </div>
    </div>
</article>

@code {
    [Parameter]
    public PropertyDto? Property { get; set; }
}
```

#### **Paso 3: Filtros (Componente)**

```razor
@* Shared/PropertyFilterPanel.razor *@
@namespace cima.Blazor.Shared
@using cima.Domain.Shared.Dtos

<div class="bg-white rounded-lg shadow p-6">
    <h2 class="text-lg font-bold text-gray-900 mb-6">Filtros</h2>

    <!-- Buscar -->
    <div class="mb-6">
        <label for="search" class="block text-sm font-semibold text-gray-700 mb-2">
            Buscar por título
        </label>
        <input 
            id="search"
            type="text" 
            @bind="FilterData.Title"
            @bind:event="oninput"
            placeholder="Ej: Apartamento en Miraflores"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
            aria-label="Buscar propiedades por título"
        />
    </div>

    <!-- Bedrooms -->
    <div class="mb-6">
        <label for="bedrooms" class="block text-sm font-semibold text-gray-700 mb-2">
            Recámaras
        </label>
        <select 
            id="bedrooms"
            @bind="FilterData.Bedrooms"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
            aria-label="Filtrar por cantidad de recámaras">
            <option value="">Todas</option>
            <option value="1">1 Recámara</option>
            <option value="2">2 Recámaras</option>
            <option value="3">3 Recámaras</option>
            <option value="4">4+ Recámaras</option>
        </select>
    </div>

    <!-- Price Range -->
    <div class="mb-6">
        <label class="block text-sm font-semibold text-gray-700 mb-2">
            Rango de Precio
        </label>
        <div class="space-y-2">
            <input 
                type="number" 
                @bind="FilterData.PriceMin"
                placeholder="Mínimo"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none text-sm"
                aria-label="Precio mínimo"
            />
            <input 
                type="number" 
                @bind="FilterData.PriceMax"
                placeholder="Máximo"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none text-sm"
                aria-label="Precio máximo"
            />
        </div>
    </div>

    <!-- Buttons -->
    <div class="flex gap-2">
        <button 
            @onclick="ApplyFiltersAsync"
            class="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors font-medium">
            <i class="fas fa-search mr-2"></i> Filtrar
        </button>
        <button 
            @onclick="ResetFiltersAsync"
            class="flex-1 bg-gray-200 text-gray-900 px-4 py-2 rounded-lg hover:bg-gray-300 transition-colors font-medium">
            <i class="fas fa-redo mr-2"></i> Limpiar
        </button>
    </div>
</div>

@code {
    private PropertyFilterDto FilterData = new();

    [Parameter]
    public EventCallback OnFilterChanged { get; set; }

    public PropertyFilterDto GetFilter() => FilterData;

    private async Task ApplyFiltersAsync()
    {
        await OnFilterChanged.InvokeAsync();
    }

    private async Task ResetFiltersAsync()
    {
        FilterData = new PropertyFilterDto();
        await OnFilterChanged.InvokeAsync();
    }
}
```

#### **Paso 4: Detalle de Propiedad**

```razor
@* Pages/Properties/Detail.razor *@
@page "/properties/{PropertyId:guid}"
@using cima.Domain.Shared.Dtos
@inject IPropertyAppService PropertyAppService
@inject IContactRequestAppService ContactRequestAppService
@inject MessageService MessageService

<PageTitle>@Property?.Title | CIMA</PageTitle>

@if (Loading)
{
    <div class="flex items-center justify-center h-screen">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
    </div>
}
else if (Property == null)
{
    <div class="container mx-auto px-4 py-12 text-center">
        <h1 class="text-3xl font-bold text-gray-900 mb-4">Propiedad no encontrada</h1>
        <a href="/properties" class="text-blue-600 hover:underline">? Volver al catálogo</a>
    </div>
}
else
{
    <div class="bg-white">
        <!-- Galería -->
        <PropertyImageGallery Images="Property.Images" />

        <!-- Contenido -->
        <div class="container mx-auto px-4 py-8">
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
                
                <!-- Info Principal -->
                <div class="lg:col-span-2">
                    <h1 class="text-4xl font-bold text-gray-900 mb-2">@Property.Title</h1>
                    <p class="text-gray-600 text-lg mb-6">
                        <i class="fas fa-map-marker-alt text-red-500 mr-2"></i>
                        @Property.Location
                    </p>

                    <div class="mb-8 pb-8 border-b border-gray-200">
                        <p class="text-4xl font-bold text-green-600">
                            $@Property.Price.ToString("N0")
                        </p>
                    </div>

                    <!-- Características -->
                    <div class="grid grid-cols-3 gap-4 mb-8">
                        <div class="bg-blue-50 p-6 rounded-lg text-center">
                            <i class="fas fa-door-open text-3xl text-blue-600 mb-2"></i>
                            <p class="text-3xl font-bold text-gray-900">@Property.Bedrooms</p>
                            <p class="text-gray-600">Recámaras</p>
                        </div>
                        <div class="bg-blue-50 p-6 rounded-lg text-center">
                            <i class="fas fa-bath text-3xl text-blue-600 mb-2"></i>
                            <p class="text-3xl font-bold text-gray-900">@Property.Bathrooms</p>
                            <p class="text-gray-600">Baños</p>
                        </div>
                        <div class="bg-blue-50 p-6 rounded-lg text-center">
                            <i class="fas fa-ruler-combined text-3xl text-blue-600 mb-2"></i>
                            <p class="text-3xl font-bold text-gray-900">@Property.Area</p>
                            <p class="text-gray-600">m²</p>
                        </div>
                    </div>

                    <!-- Descripción -->
                    <section>
                        <h2 class="text-2xl font-bold text-gray-900 mb-4">Descripción</h2>
                        <p class="text-gray-700 leading-relaxed whitespace-pre-wrap">
                            @Property.Description
                        </p>
                    </section>
                </div>

                <!-- Sidebar -->
                <aside class="lg:col-span-1">
                    
                    <!-- Arquitecto -->
                    @if (Property.Architect != null)
                    {
                        <div class="bg-gray-50 rounded-lg p-6 mb-6">
                            <h3 class="text-lg font-bold text-gray-900 mb-4">Arquitecto</h3>
                            <div class="flex items-center gap-4 mb-4">
                                <div class="w-16 h-16 bg-blue-200 rounded-full flex items-center justify-center">
                                    <i class="fas fa-user text-3xl text-blue-600"></i>
                                </div>
                                <div>
                                    <p class="font-semibold text-gray-900">
                                        @Property.Architect.UserName
                                    </p>
                                    @if (!string.IsNullOrEmpty(Property.Architect.Bio))
                                    {
                                        <p class="text-sm text-gray-600">
                                            @Property.Architect.Bio
                                        </p>
                                    }
                                </div>
                            </div>
                            <a 
                                href="/architects/@Property.Architect.Id" 
                                class="block w-full bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors text-center font-medium">
                                Ver Portfolio
                            </a>
                        </div>
                    }

                    <!-- Botón Contacto -->
                    <button 
                        @onclick="OpenContactFormAsync"
                        class="w-full bg-green-600 text-white px-6 py-3 rounded-lg hover:bg-green-700 transition-colors font-bold text-lg mb-4">
                        <i class="fas fa-envelope mr-2"></i> Contactar
                    </button>

                    <!-- Share -->
                    <div class="flex gap-2">
                        <button class="flex-1 bg-blue-500 text-white px-4 py-2 rounded-lg hover:bg-blue-600 transition-colors" title="Compartir en Facebook">
                            <i class="fab fa-facebook"></i>
                        </button>
                        <button class="flex-1 bg-blue-400 text-white px-4 py-2 rounded-lg hover:bg-blue-500 transition-colors" title="Compartir en Twitter">
                            <i class="fab fa-twitter"></i>
                        </button>
                        <button class="flex-1 bg-green-500 text-white px-4 py-2 rounded-lg hover:bg-green-600 transition-colors" title="Compartir por WhatsApp">
                            <i class="fab fa-whatsapp"></i>
                        </button>
                    </div>
                </aside>
            </div>
        </div>
    </div>

    <!-- Modal: Contact Form -->
    @if (ShowContactForm)
    {
        <ContactFormModal 
            PropertyId="Property.Id"
            ArchitectName="Property.Architect?.UserName"
            OnClose="CloseContactForm"
            OnSuccess="HandleContactFormSuccess"
        />
    }
}

@code {
    [Parameter]
    public Guid PropertyId { get; set; }

    private PropertyDto? Property;
    private bool Loading = true;
    private bool ShowContactForm = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadPropertyAsync();
    }

    private async Task LoadPropertyAsync()
    {
        try
        {
            Loading = true;
            Property = await PropertyAppService.GetAsync(PropertyId);
        }
        catch (Exception ex)
        {
            await MessageService.Error("Error al cargar la propiedad");
            Logger.LogError(ex, "Error loading property");
        }
        finally
        {
            Loading = false;
        }
    }

    private async Task OpenContactFormAsync()
    {
        ShowContactForm = true;
    }

    private void CloseContactForm()
    {
        ShowContactForm = false;
    }

    private async Task HandleContactFormSuccess()
    {
        CloseContactForm();
        await MessageService.Success("¡Gracias! Tu mensaje fue enviado exitosamente");
    }
}
```

---

## ?? CONVENCIONES TAILWIND CSS

```razor
@* ? Correcto: Clases Tailwind, responsive-first *@
<div class="flex flex-col md:flex-row gap-4 p-6 bg-gray-50 rounded-lg hover:shadow-lg transition-shadow">
    <h2 class="text-2xl md:text-3xl font-bold text-gray-900">Título</h2>
    <p class="text-gray-600 text-sm md:text-base">Descripción</p>
</div>

@* ? Evitar: CSS inline, clases custom sin Tailwind *@
<div style="display: flex; gap: 10px; padding: 20px; background-color: #f3f4f6;">
    <h2 style="font-size: 24px; font-weight: bold;">Título</h2>
</div>

@* Responsive Breakpoints *@
Hidden on mobile, visible on desktop: hidden lg:block
Single column on mobile, 3 on desktop: grid-cols-1 lg:grid-cols-3
Padding varies: px-4 md:px-6 lg:px-8
```

---

## ? ACCESIBILIDAD (A11y)

### **Checklist de Accesibilidad**

- [ ] Toda imagen tiene `alt="descripción significativa"`
- [ ] Labels asociados a inputs: `<label for="id">`
- [ ] Botones tienen texto descriptivo o `aria-label`
- [ ] Formularios validar y mostrar errores
- [ ] Contraste suficiente (WCAG AA mínimo)
- [ ] Navegación con teclado (Tab funciona)
- [ ] ARIA roles cuando sea necesario: `role="alert"`, `aria-live="polite"`
- [ ] Iconos decorativos: `aria-hidden="true"`

### **Ejemplos**

```razor
@* ? Correcto: Accesible *@
<label for="email" class="block text-sm font-medium text-gray-700 mb-2">
    Email
</label>
<input 
    id="email"
    type="email"
    required
    aria-required="true"
    aria-invalid="@(HasError ? "true" : "false")"
    class="w-full px-4 py-2 border border-gray-300 rounded-lg..."
/>

<button 
    @onclick="Submit"
    aria-label="Enviar formulario"
    class="bg-blue-600 text-white...">
    <i class="fas fa-send" aria-hidden="true"></i> Enviar
</button>

@* Mensajes de error accesibles *@
@if (Errors.Any())
{
    <div role="alert" class="bg-red-50 border-l-4 border-red-400 p-4">
        <ul class="list-disc list-inside text-red-700">
            @foreach (var error in Errors)
            {
                <li>@error</li>
            }
        </ul>
    </div>
}

@* ? Evitar *@
<img src="property.jpg" /> @* Sin alt text *@
<input type="email" /> @* Sin label *@
<button @onclick="Submit"> @* Sin aria-label, solo ícono *@
    <i class="fas fa-send"></i>
</button>
```

---

## ?? CHECKLIST: Antes de Cada PR (Frontend)

- [ ] **Responsive:** Testea en mobile, tablet, desktop
- [ ] **Accesibilidad:** DevTools ? Lighthouse > 90
- [ ] **Performance:** Imágenes lazy-loaded, componentes no bloquean
- [ ] **Convenciones:** Tailwind solamente (sin CSS inline)
- [ ] **Validación:** Client-side + feedback al usuario
- [ ] **Error handling:** Mensajes claros, retry buttons
- [ ] **Loading states:** UX clara mientras se carga
- [ ] **URLs:** Rutas amigables (`/properties/{id}` no `/property?id=`)
- [ ] **Commits:** Semántico en español, PR bien descrita
- [ ] **Compilable:** `dotnet build` sin warnings

---

## ?? COMPONENTES REUTILIZABLES

### **Plantilla: Componente Base**

```razor
@namespace cima.Blazor.Shared

<!-- Componente genérico -->
<div class="...">
    <!-- Contenido -->
    @ChildContent
</div>

@code {
    /// <summary>
    /// Contenido interno del componente
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Callback cuando ocurre un evento
    /// </summary>
    [Parameter]
    public EventCallback<T> OnEvent { get; set; }
}
```

### **Ejemplos**

- `PropertyCard.razor` ? Card de propiedad
- `PropertyImageGallery.razor` ? Galería de imágenes
- `FilterPanel.razor` ? Filtros búsqueda
- `Pagination.razor` ? Paginación reutilizable
- `ContactForm.razor` ? Formulario de contacto
- `LoadingSpinner.razor` ? Spinner de carga

---

## ?? COMANDOS QUE USARÁS FRECUENTEMENTE

```powershell
# Build
dotnet build

# Run locally (dev)
dotnet run --project src/cima.Blazor

# DevTools en navegador
F12 ? Accessibility ? Lighthouse ? Run audit

# Inspeccionar elemento
Right-click ? Inspect ? Check alt text, labels, etc.
```

---

## ? ANTIPATRONES: NUNCA HAGAS ESTO

```razor
@* ? CSS inline en componentes *@
<div style="display: flex; padding: 20px; background-color: #f3f4f6;">

@* ? Correcto: Tailwind *@
<div class="flex p-6 bg-gray-50">

@* ? Imágenes sin alt *@
<img src="property.jpg" />

@* ? Correcto *@
<img src="property.jpg" alt="Apartamento en Miraflores - 3 recámaras" loading="lazy" />

@* ? Inputs sin labels *@
<input type="email" placeholder="Email" />

@* ? Correcto *@
<label for="email">Email</label>
<input id="email" type="email" aria-label="Tu email" />

@* ? Valores hardcodeados *@
<h1 style="font-size: 32px;">Título</h1>

@* ? Correcto: Tailwind classes *@
<h1 class="text-4xl font-bold">Título</h1>

@* ? sin lazy loading en imágenes *@
<img src="..." />

@* ? Correcto *@
<img src="..." loading="lazy" />
```

---

## ?? REFERENCIAS

- **Blazor Docs:** https://learn.microsoft.com/aspnet/core/blazor
- **Tailwind CSS:** https://tailwindcss.com/docs
- **Blazorise Docs:** https://blazorise.com/
- **WCAG Accessibility:** https://www.w3.org/WAI/WCAG21/quickref/
- **MDN Web Docs:** https://developer.mozilla.org/

---

**Última actualización:** Setup inicial  
**Versión:** 1.0  
**Para:** Google Gemini

