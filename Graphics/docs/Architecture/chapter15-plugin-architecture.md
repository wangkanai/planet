# Chapter 15: Plugin Architecture

Building extensible graphics processing systems requires careful architectural planning to enable third-party developers
to add functionality without compromising system stability or security. Modern plugin architectures must balance
flexibility with performance, provide clear extension points while maintaining encapsulation, and ensure that poorly
written plugins cannot destabilize the host application. This chapter explores the design and implementation of a robust
plugin system using the Managed Extensibility Framework (MEF) in .NET 9.0, covering security isolation, dynamic
discovery, and API design patterns that enable professional-grade extensibility.

## 15.1 MEF-Based Extensibility

The Managed Extensibility Framework provides a powerful foundation for building plugin systems in .NET applications. *
*MEF's attribute-based programming model enables declarative composition**, where plugins announce their capabilities
through metadata and the framework handles discovery, instantiation, and dependency injection. In graphics processing
applications, this approach allows filters, effects, format handlers, and analysis tools to be added without modifying
core application code.

### Understanding MEF composition architecture

MEF operates on three fundamental concepts that form the basis of its composition model. **Parts** represent components
that can be composed, typically plugins or host services. **Exports** declare capabilities that parts provide to the
system, while **imports** specify dependencies that parts require. The composition container brings these elements
together, satisfying imports with matching exports based on contracts and metadata.

```csharp
// Core plugin interfaces
public interface IGraphicsFilter
{
    string Name { get; }
    string Description { get; }
    Version Version { get; }
    Task<ImageData> ProcessAsync(ImageData input, IFilterContext context);
    IFilterConfiguration CreateDefaultConfiguration();
    bool ValidateConfiguration(IFilterConfiguration configuration);
}

public interface IFilterMetadata
{
    string Name { get; }
    string Category { get; }
    string[] SupportedFormats { get; }
    bool SupportsGpu { get; }
    int ProcessingPriority { get; }
}

// Plugin implementation with MEF attributes
[Export(typeof(IGraphicsFilter))]
[ExportMetadata("Name", "Advanced Sharpen")]
[ExportMetadata("Category", "Enhancement")]
[ExportMetadata("SupportedFormats", new[] { "JPEG", "PNG", "TIFF" })]
[ExportMetadata("SupportsGpu", true)]
[ExportMetadata("ProcessingPriority", 100)]
public class AdvancedSharpenFilter : IGraphicsFilter
{
    private readonly ILogger<AdvancedSharpenFilter> _logger;
    private readonly IMemoryAllocator _memoryAllocator;

    [ImportingConstructor]
    public AdvancedSharpenFilter(
        ILogger<AdvancedSharpenFilter> logger,
        IMemoryAllocator memoryAllocator)
    {
        _logger = logger;
        _memoryAllocator = memoryAllocator;
    }

    public string Name => "Advanced Sharpen";
    public string Description => "GPU-accelerated unsharp mask with edge preservation";
    public Version Version => new Version(2, 0, 0);

    public async Task<ImageData> ProcessAsync(ImageData input, IFilterContext context)
    {
        _logger.LogInformation("Processing image with Advanced Sharpen filter");

        // Validate input
        if (!ValidateInput(input))
        {
            throw new FilterException("Invalid input data");
        }

        // Get configuration
        var config = context.Configuration as SharpenConfiguration
            ?? CreateDefaultConfiguration() as SharpenConfiguration;

        // Process based on available acceleration
        if (context.GpuContext?.IsAvailable == true && config.UseGpuAcceleration)
        {
            return await ProcessGpuAsync(input, config, context.GpuContext);
        }

        return await ProcessCpuAsync(input, config);
    }

    private async Task<ImageData> ProcessGpuAsync(
        ImageData input,
        SharpenConfiguration config,
        IGpuContext gpu)
    {
        using var inputBuffer = gpu.AllocateBuffer(input.Width * input.Height * 4);
        using var outputBuffer = gpu.AllocateBuffer(input.Width * input.Height * 4);

        // Upload to GPU
        await gpu.UploadAsync(input.PixelData, inputBuffer);

        // Compile and execute compute shader
        var shader = await gpu.CompileShaderAsync(@"
            [numthreads(16, 16, 1)]
            void SharpenKernel(uint3 id : SV_DispatchThreadID)
            {
                // Unsharp mask implementation
                float4 center = inputTexture.Load(id);
                float4 blur = GaussianBlur(id, radius);
                float4 sharp = center + (center - blur) * amount;

                // Edge preservation
                float edge = EdgeDetection(id);
                float4 result = lerp(center, sharp, 1.0 - edge * edgeThreshold);

                outputTexture[id] = saturate(result);
            }
        ");

        var parameters = new ShaderParameters
        {
            ["radius"] = config.Radius,
            ["amount"] = config.Amount,
            ["edgeThreshold"] = config.EdgeThreshold
        };

        await gpu.DispatchAsync(shader, inputBuffer, outputBuffer, parameters);

        // Download result
        var result = new byte[outputBuffer.Size];
        await gpu.DownloadAsync(outputBuffer, result);

        return new ImageData(input.Width, input.Height, input.Format, result);
    }

    public IFilterConfiguration CreateDefaultConfiguration()
    {
        return new SharpenConfiguration
        {
            Radius = 1.5f,
            Amount = 0.8f,
            EdgeThreshold = 0.1f,
            UseGpuAcceleration = true
        };
    }

    public bool ValidateConfiguration(IFilterConfiguration configuration)
    {
        if (configuration is not SharpenConfiguration config)
            return false;

        return config.Radius > 0 && config.Radius <= 10
            && config.Amount >= 0 && config.Amount <= 5
            && config.EdgeThreshold >= 0 && config.EdgeThreshold <= 1;
    }
}
```

### Implementing a sophisticated plugin host

The plugin host manages the MEF composition container and provides essential services to plugins. **A well-designed host
implements lazy loading to minimize startup time**, maintains plugin lifecycle management, and provides comprehensive
error isolation to prevent plugin failures from affecting system stability.

```csharp
public class PluginHost : IPluginHost, IDisposable
{
    private readonly CompositionContainer _container;
    private readonly AggregateCatalog _catalog;
    private readonly Dictionary<string, Lazy<IGraphicsFilter, IFilterMetadata>> _filters;
    private readonly ILogger<PluginHost> _logger;
    private readonly PluginSecurityManager _securityManager;

    [ImportMany]
    public IEnumerable<Lazy<IGraphicsFilter, IFilterMetadata>> AvailableFilters { get; set; }

    public PluginHost(PluginHostConfiguration configuration, ILogger<PluginHost> logger)
    {
        _logger = logger;
        _catalog = new AggregateCatalog();
        _filters = new Dictionary<string, Lazy<IGraphicsFilter, IFilterMetadata>>();
        _securityManager = new PluginSecurityManager(configuration.SecurityPolicy);

        // Add host assembly exports
        _catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));

        // Create container with custom export providers
        var batch = new CompositionBatch();
        batch.AddExportedValue<ILogger>(logger);
        batch.AddExportedValue<IMemoryAllocator>(new PooledMemoryAllocator());
        batch.AddExportedValue<IPluginHost>(this);

        _container = new CompositionContainer(_catalog, CompositionOptions.DisableSilentRejection);
        _container.Compose(batch);

        // Load plugins from configured directories
        foreach (var pluginDirectory in configuration.PluginDirectories)
        {
            LoadPluginsFromDirectory(pluginDirectory);
        }

        // Compose to populate ImportMany properties
        _container.ComposeParts(this);

        // Index filters for fast lookup
        foreach (var filter in AvailableFilters)
        {
            _filters[filter.Metadata.Name] = filter;
            _logger.LogInformation($"Loaded filter: {filter.Metadata.Name} v{filter.Value.Version}");
        }
    }

    public async Task<IGraphicsFilter> GetFilterAsync(string name)
    {
        if (!_filters.TryGetValue(name, out var lazyFilter))
        {
            throw new FilterNotFoundException($"Filter '{name}' not found");
        }

        try
        {
            // Lazy instantiation with error handling
            var filter = await Task.Run(() => lazyFilter.Value);

            // Validate filter after instantiation
            if (!await ValidateFilterAsync(filter))
            {
                throw new FilterValidationException($"Filter '{name}' failed validation");
            }

            return filter;
        }
        catch (CompositionException ex)
        {
            _logger.LogError(ex, $"Failed to instantiate filter '{name}'");
            throw new FilterLoadException($"Could not load filter '{name}'", ex);
        }
    }

    public IEnumerable<FilterInfo> GetAvailableFilters(FilterCategory? category = null)
    {
        var query = AvailableFilters.AsEnumerable();

        if (category.HasValue)
        {
            var categoryName = category.Value.ToString();
            query = query.Where(f => f.Metadata.Category == categoryName);
        }

        return query.Select(f => new FilterInfo
        {
            Name = f.Metadata.Name,
            Category = f.Metadata.Category,
            SupportedFormats = f.Metadata.SupportedFormats,
            SupportsGpu = f.Metadata.SupportsGpu,
            ProcessingPriority = f.Metadata.ProcessingPriority
        }).OrderBy(f => f.ProcessingPriority);
    }

    private void LoadPluginsFromDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            _logger.LogWarning($"Plugin directory not found: {directory}");
            return;
        }

        try
        {
            // Use SafeDirectoryCatalog for error isolation
            var directoryCatalog = new SafeDirectoryCatalog(directory, "*.dll", _logger);
            _catalog.Catalogs.Add(directoryCatalog);

            _logger.LogInformation($"Loaded plugins from: {directory}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load plugins from: {directory}");
        }
    }

    private async Task<bool> ValidateFilterAsync(IGraphicsFilter filter)
    {
        try
        {
            // Create test context
            var testImage = CreateTestImage();
            var testContext = new FilterContext
            {
                Configuration = filter.CreateDefaultConfiguration(),
                GpuContext = null,
                CancellationToken = CancellationToken.None
            };

            // Attempt to process test image
            var result = await filter.ProcessAsync(testImage, testContext);

            // Validate result
            return result != null
                && result.Width == testImage.Width
                && result.Height == testImage.Height
                && result.PixelData?.Length > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Filter validation failed for: {filter.Name}");
            return false;
        }
    }

    public void Dispose()
    {
        _container?.Dispose();
        _catalog?.Dispose();
    }
}

// Safe directory catalog that isolates failures
public class SafeDirectoryCatalog : ComposablePartCatalog
{
    private readonly List<ComposablePartCatalog> _catalogs = new();
    private readonly ILogger _logger;

    public SafeDirectoryCatalog(string directory, string searchPattern, ILogger logger)
    {
        _logger = logger;

        foreach (var file in Directory.GetFiles(directory, searchPattern))
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
                var catalog = new AssemblyCatalog(assembly);

                // Verify catalog has exports
                if (catalog.Parts.Any())
                {
                    _catalogs.Add(catalog);
                    _logger.LogDebug($"Loaded assembly: {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to load assembly: {file}");
            }
        }
    }

    public override IQueryable<ComposablePartDefinition> Parts
    {
        get { return _catalogs.SelectMany(c => c.Parts).AsQueryable(); }
    }
}
```

### Advanced MEF patterns for graphics processing

Professional graphics applications require sophisticated composition patterns beyond basic import/export relationships.
**Metadata-based filtering enables intelligent plugin selection based on image characteristics**, while lazy loading
with metadata prevents unnecessary plugin instantiation. The composition system must handle complex scenarios including
multiple versions of the same plugin, conditional exports based on system capabilities, and dynamic recomposition when
plugins are added or removed at runtime.

```csharp
// Advanced composition with metadata filtering
public class SmartFilterSelector
{
    private readonly IEnumerable<Lazy<IGraphicsFilter, IFilterMetadata>> _filters;
    private readonly ISystemCapabilities _capabilities;

    [ImportMany]
    public SmartFilterSelector(
        IEnumerable<Lazy<IGraphicsFilter, IFilterMetadata>> filters,
        ISystemCapabilities capabilities)
    {
        _filters = filters;
        _capabilities = capabilities;
    }

    public IGraphicsFilter SelectOptimalFilter(
        string filterName,
        ImageInfo imageInfo,
        ProcessingPreferences preferences)
    {
        // Find all filters matching the name
        var candidates = _filters
            .Where(f => f.Metadata.Name == filterName)
            .ToList();

        if (!candidates.Any())
        {
            throw new FilterNotFoundException($"No filter found with name: {filterName}");
        }

        // Score each candidate based on suitability
        var scoredCandidates = candidates
            .Select(f => new
            {
                Filter = f,
                Score = CalculateFilterScore(f.Metadata, imageInfo, preferences)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ToList();

        if (!scoredCandidates.Any())
        {
            throw new FilterNotSuitableException(
                $"No suitable variant of '{filterName}' for the given image");
        }

        // Return the highest scoring filter
        return scoredCandidates.First().Filter.Value;
    }

    private int CalculateFilterScore(
        IFilterMetadata metadata,
        ImageInfo imageInfo,
        ProcessingPreferences preferences)
    {
        int score = 100; // Base score

        // Format compatibility
        if (!metadata.SupportedFormats.Contains(imageInfo.Format))
        {
            return 0; // Incompatible
        }

        // GPU preference alignment
        if (preferences.PreferGpuAcceleration && metadata.SupportsGpu && _capabilities.HasGpu)
        {
            score += 50;
        }
        else if (!preferences.PreferGpuAcceleration && !metadata.SupportsGpu)
        {
            score += 30;
        }

        // Processing priority (lower is better)
        score -= metadata.ProcessingPriority / 10;

        // Image size considerations
        if (imageInfo.Width * imageInfo.Height > 4096 * 4096)
        {
            // Large images benefit from GPU acceleration
            if (metadata.SupportsGpu)
            {
                score += 40;
            }
        }

        return score;
    }
}

// Conditional exports based on system capabilities
[Export(typeof(IGraphicsFilter))]
[ExportMetadata("Name", "Neural Style Transfer")]
[ExportMetadata("RequiresCapability", "CUDA")]
public class NeuralStyleTransferFilter : IGraphicsFilter
{
    private readonly Lazy<ICudaContext> _cudaContext;

    [ImportingConstructor]
    public NeuralStyleTransferFilter([Import(AllowDefault = true)] Lazy<ICudaContext> cudaContext)
    {
        _cudaContext = cudaContext;
    }

    public bool IsAvailable => _cudaContext?.Value != null;

    public async Task<ImageData> ProcessAsync(ImageData input, IFilterContext context)
    {
        if (!IsAvailable)
        {
            throw new FilterNotAvailableException("CUDA is required for Neural Style Transfer");
        }

        // Implementation using CUDA
        return await ProcessWithCudaAsync(input, context);
    }
}
```

## 15.2 Security and Isolation

Plugin systems introduce significant security challenges as they execute third-party code within the host application's
process. **Modern .NET provides multiple isolation mechanisms ranging from AppDomains (legacy) to AssemblyLoadContext
and even process isolation**, each offering different tradeoffs between security, performance, and functionality.
Graphics processing plugins require particular attention to security due to their access to potentially sensitive image
data and system resources.

### AssemblyLoadContext for plugin isolation

.NET Core and .NET 5+ replaced AppDomains with AssemblyLoadContext (ALC), providing a lighter-weight mechanism for
assembly isolation. **Each plugin can be loaded into its own ALC, enabling independent versioning and unloading**, while
shared dependencies can be resolved through a careful hierarchy of contexts. This approach prevents version conflicts
and allows plugins to be updated or removed without restarting the host application.

```csharp
public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginPath;
    private readonly HashSet<string> _sharedAssemblies;
    private readonly ILogger _logger;

    public PluginLoadContext(
        string pluginPath,
        HashSet<string> sharedAssemblies,
        ILogger logger) : base(isCollectible: true)
    {
        _pluginPath = pluginPath;
        _resolver = new AssemblyDependencyResolver(pluginPath);
        _sharedAssemblies = sharedAssemblies;
        _logger = logger;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        // Check if this should be loaded from the shared context
        if (_sharedAssemblies.Contains(assemblyName.Name))
        {
            _logger.LogDebug($"Loading shared assembly: {assemblyName.Name}");
            return null; // Delegate to default context
        }

        // Try to load from plugin directory
        string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            _logger.LogDebug($"Loading plugin assembly: {assemblyName.Name} from {assemblyPath}");
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            _logger.LogDebug($"Loading unmanaged library: {unmanagedDllName} from {libraryPath}");
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}

// Secure plugin loader with validation
public class SecurePluginLoader : IPluginLoader
{
    private readonly Dictionary<string, PluginLoadContext> _loadContexts = new();
    private readonly PluginSecurityPolicy _securityPolicy;
    private readonly ILogger<SecurePluginLoader> _logger;
    private readonly HashSet<string> _sharedAssemblies;

    public SecurePluginLoader(
        PluginSecurityPolicy securityPolicy,
        ILogger<SecurePluginLoader> logger)
    {
        _securityPolicy = securityPolicy;
        _logger = logger;

        // Define shared assemblies that should not be isolated
        _sharedAssemblies = new HashSet<string>
        {
            "System.Runtime",
            "System.Collections",
            "System.Linq",
            "Microsoft.Extensions.Logging.Abstractions",
            typeof(IGraphicsFilter).Assembly.GetName().Name
        };
    }

    public async Task<LoadedPlugin> LoadPluginAsync(string pluginPath)
    {
        // Validate plugin before loading
        var validationResult = await ValidatePluginAsync(pluginPath);
        if (!validationResult.IsValid)
        {
            throw new PluginValidationException(
                $"Plugin validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Create isolated load context
        var loadContext = new PluginLoadContext(pluginPath, _sharedAssemblies, _logger);

        try
        {
            // Load assembly
            var assembly = loadContext.LoadFromAssemblyPath(pluginPath);

            // Verify assembly attributes
            var attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
            if (!VerifyAssemblyMetadata(attributes))
            {
                throw new SecurityException("Assembly metadata verification failed");
            }

            // Find and instantiate plugin types
            var pluginTypes = FindPluginTypes(assembly);
            var instances = new List<IGraphicsFilter>();

            foreach (var type in pluginTypes)
            {
                // Create instance with security constraints
                var instance = CreateSecureInstance(type, loadContext);
                instances.Add(instance);
            }

            // Register load context
            _loadContexts[pluginPath] = loadContext;

            return new LoadedPlugin
            {
                Path = pluginPath,
                Assembly = assembly,
                LoadContext = loadContext,
                Filters = instances,
                Metadata = ExtractPluginMetadata(assembly)
            };
        }
        catch (Exception ex)
        {
            // Cleanup on failure
            loadContext.Unload();
            throw new PluginLoadException($"Failed to load plugin: {pluginPath}", ex);
        }
    }

    private async Task<ValidationResult> ValidatePluginAsync(string pluginPath)
    {
        var result = new ValidationResult();

        // File existence and permissions
        if (!File.Exists(pluginPath))
        {
            result.AddError("Plugin file not found");
            return result;
        }

        // Digital signature verification
        if (_securityPolicy.RequireSignedAssemblies)
        {
            if (!await VerifyAssemblySignatureAsync(pluginPath))
            {
                result.AddError("Assembly is not properly signed");
            }
        }

        // Hash verification against whitelist
        if (_securityPolicy.UseWhitelist)
        {
            var hash = await ComputeFileHashAsync(pluginPath);
            if (!_securityPolicy.WhitelistedHashes.Contains(hash))
            {
                result.AddError("Assembly hash not in whitelist");
            }
        }

        // Scan for suspicious patterns
        if (_securityPolicy.EnableSecurityScanning)
        {
            var scanResult = await ScanForSuspiciousPatternsAsync(pluginPath);
            if (!scanResult.IsSafe)
            {
                result.AddError($"Security scan failed: {scanResult.Reason}");
            }
        }

        return result;
    }

    private IGraphicsFilter CreateSecureInstance(Type type, PluginLoadContext context)
    {
        // Wrap plugin instance in security proxy
        var instance = Activator.CreateInstance(type) as IGraphicsFilter;

        return new SecureFilterProxy(instance, _securityPolicy);
    }

    public async Task UnloadPluginAsync(string pluginPath)
    {
        if (_loadContexts.TryGetValue(pluginPath, out var context))
        {
            // Trigger garbage collection to ensure unloading
            context.Unload();

            // Wait for unload to complete
            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                if (context.IsCollectible && await IsUnloadedAsync(context))
                {
                    _loadContexts.Remove(pluginPath);
                    _logger.LogInformation($"Successfully unloaded plugin: {pluginPath}");
                    return;
                }

                await Task.Delay(100);
            }

            _logger.LogWarning($"Plugin unload may not have completed: {pluginPath}");
        }
    }
}
```

### Implementing security boundaries and permissions

Security boundaries must be enforced at multiple levels to protect both the host application and user data. **Resource
access control prevents plugins from consuming excessive memory or CPU**, while data access restrictions ensure plugins
only access authorized images and cannot exfiltrate sensitive information. The security system must be transparent
enough for legitimate plugins while preventing malicious behavior.

```csharp
// Comprehensive security proxy for filter execution
public class SecureFilterProxy : IGraphicsFilter
{
    private readonly IGraphicsFilter _innerFilter;
    private readonly PluginSecurityPolicy _policy;
    private readonly ResourceMonitor _resourceMonitor;
    private readonly AccessController _accessController;

    public SecureFilterProxy(
        IGraphicsFilter innerFilter,
        PluginSecurityPolicy policy)
    {
        _innerFilter = innerFilter;
        _policy = policy;
        _resourceMonitor = new ResourceMonitor(policy.ResourceLimits);
        _accessController = new AccessController(policy.AccessRules);
    }

    public string Name => _innerFilter.Name;
    public string Description => _innerFilter.Description;
    public Version Version => _innerFilter.Version;

    public async Task<ImageData> ProcessAsync(ImageData input, IFilterContext context)
    {
        // Check permissions
        _accessController.ValidateAccess(input, context);

        // Create restricted context
        var restrictedContext = new RestrictedFilterContext(context, _policy);

        // Monitor resource usage
        using var resourceScope = _resourceMonitor.BeginScope(_innerFilter.Name);

        try
        {
            // Execute with timeout
            using var cts = new CancellationTokenSource(_policy.ExecutionTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cts.Token, context.CancellationToken);

            restrictedContext.CancellationToken = linkedCts.Token;

            // Run in constrained execution environment
            var result = await Task.Run(async () =>
            {
                // Set thread priority
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                // Execute filter
                return await _innerFilter.ProcessAsync(input, restrictedContext);
            }, linkedCts.Token);

            // Validate output
            ValidateOutput(result, input);

            return result;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            throw new FilterTimeoutException(
                $"Filter '{_innerFilter.Name}' exceeded timeout of {_policy.ExecutionTimeout}");
        }
        finally
        {
            // Log resource usage
            var usage = resourceScope.GetUsage();
            if (usage.PeakMemoryMB > _policy.ResourceLimits.MaxMemoryMB * 0.8)
            {
                LogWarning($"Filter '{_innerFilter.Name}' used {usage.PeakMemoryMB}MB memory");
            }
        }
    }

    private void ValidateOutput(ImageData output, ImageData input)
    {
        // Ensure output is reasonable
        if (output == null)
        {
            throw new FilterException("Filter returned null output");
        }

        // Check for size bombs
        var outputSize = output.Width * output.Height * 4; // Assuming 4 bytes per pixel
        var inputSize = input.Width * input.Height * 4;

        if (outputSize > inputSize * _policy.MaxOutputSizeRatio)
        {
            throw new SecurityException(
                $"Filter output size ({outputSize}) exceeds allowed ratio to input size");
        }

        // Verify data integrity
        if (output.PixelData == null || output.PixelData.Length == 0)
        {
            throw new FilterException("Filter returned empty pixel data");
        }
    }

    public IFilterConfiguration CreateDefaultConfiguration()
    {
        // Wrap configuration creation in try-catch
        try
        {
            return _innerFilter.CreateDefaultConfiguration();
        }
        catch (Exception ex)
        {
            throw new FilterException(
                $"Filter '{_innerFilter.Name}' failed to create default configuration", ex);
        }
    }

    public bool ValidateConfiguration(IFilterConfiguration configuration)
    {
        try
        {
            return _innerFilter.ValidateConfiguration(configuration);
        }
        catch
        {
            return false; // Treat exceptions as validation failure
        }
    }
}

// Resource monitoring for plugins
public class ResourceMonitor
{
    private readonly ResourceLimits _limits;
    private readonly Dictionary<string, ResourceUsage> _usageTracking = new();

    public class ResourceScope : IDisposable
    {
        private readonly ResourceMonitor _monitor;
        private readonly string _scopeName;
        private readonly Stopwatch _stopwatch;
        private readonly long _startMemory;
        private long _peakMemory;

        public ResourceScope(ResourceMonitor monitor, string scopeName)
        {
            _monitor = monitor;
            _scopeName = scopeName;
            _stopwatch = Stopwatch.StartNew();
            _startMemory = GC.GetTotalMemory(false);
            _peakMemory = _startMemory;

            // Start monitoring thread
            Task.Run(MonitorResources);
        }

        private async Task MonitorResources()
        {
            while (!_disposed)
            {
                var currentMemory = GC.GetTotalMemory(false);
                _peakMemory = Math.Max(_peakMemory, currentMemory);

                // Check limits
                var memoryMB = (_peakMemory - _startMemory) / (1024 * 1024);
                if (memoryMB > _monitor._limits.MaxMemoryMB)
                {
                    throw new ResourceLimitExceededException(
                        $"Memory limit exceeded: {memoryMB}MB > {_monitor._limits.MaxMemoryMB}MB");
                }

                await Task.Delay(100);
            }
        }

        public ResourceUsage GetUsage()
        {
            return new ResourceUsage
            {
                ExecutionTime = _stopwatch.Elapsed,
                PeakMemoryMB = (_peakMemory - _startMemory) / (1024 * 1024),
                AverageMemoryMB = (GC.GetTotalMemory(false) - _startMemory) / (1024 * 1024)
            };
        }

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
            _stopwatch.Stop();

            var usage = GetUsage();
            _monitor._usageTracking[_scopeName] = usage;
        }
    }

    public ResourceScope BeginScope(string scopeName)
    {
        return new ResourceScope(this, scopeName);
    }
}
```

### Process isolation for untrusted plugins

For maximum security, untrusted plugins can be executed in separate processes with strictly controlled communication
channels. **This approach prevents even sophisticated attacks from compromising the host application**, though it
introduces performance overhead from inter-process communication. The architecture must carefully balance security
requirements with performance needs.

```csharp
// Out-of-process plugin host for maximum isolation
public class IsolatedPluginHost : IDisposable
{
    private readonly Process _hostProcess;
    private readonly NamedPipeServerStream _commandPipe;
    private readonly NamedPipeServerStream _dataPipe;
    private readonly ILogger<IsolatedPluginHost> _logger;
    private readonly SemaphoreSlim _commandLock = new(1);

    public IsolatedPluginHost(string pluginPath, ILogger<IsolatedPluginHost> logger)
    {
        _logger = logger;

        // Generate unique pipe names
        var pipeName = $"GraphicsPlugin_{Guid.NewGuid():N}";

        // Create named pipes for communication
        _commandPipe = new NamedPipeServerStream(
            $"{pipeName}_command",
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Message);

        _dataPipe = new NamedPipeServerStream(
            $"{pipeName}_data",
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte);

        // Start isolated host process
        _hostProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "PluginHost.exe",
                Arguments = $"--plugin \"{pluginPath}\" --pipe \"{pipeName}\"",
                UseShellExecute = false,
                CreateNoWindow = true,

                // Security restrictions
                UserName = null, // Run as current user
                LoadUserProfile = false,

                // Resource limits via Job Objects (Windows)
                // On Linux, use cgroups
            }
        };

        _hostProcess.Start();

        // Wait for connection
        var connectTask = Task.WhenAll(
            _commandPipe.WaitForConnectionAsync(),
            _dataPipe.WaitForConnectionAsync());

        if (!connectTask.Wait(TimeSpan.FromSeconds(10)))
        {
            throw new TimeoutException("Plugin host failed to connect");
        }

        // Apply additional security restrictions
        ApplyProcessRestrictions();
    }

    private void ApplyProcessRestrictions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Create Job Object with restrictions
            var job = CreateJobObject(IntPtr.Zero, null);

            var jobLimits = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = JOB_OBJECT_LIMIT_FLAGS.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE |
                                JOB_OBJECT_LIMIT_FLAGS.JOB_OBJECT_LIMIT_PROCESS_MEMORY,
                    ProcessMemoryLimit = new UIntPtr(512 * 1024 * 1024) // 512MB
                }
            };

            SetInformationJobObject(job, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation,
                ref jobLimits, Marshal.SizeOf(jobLimits));

            AssignProcessToJobObject(job, _hostProcess.Handle);
        }
        // Linux: Use cgroups for resource limits
    }

    public async Task<ImageData> ProcessAsync(
        string filterName,
        ImageData input,
        IFilterContext context)
    {
        await _commandLock.WaitAsync();
        try
        {
            // Send command
            var command = new PluginCommand
            {
                Type = CommandType.Process,
                FilterName = filterName,
                ImageInfo = new ImageInfo
                {
                    Width = input.Width,
                    Height = input.Height,
                    Format = input.Format
                }
            };

            await SendCommandAsync(command);

            // Send image data
            await SendImageDataAsync(input);

            // Send context/configuration
            await SendContextAsync(context);

            // Wait for response
            var response = await ReceiveResponseAsync();

            if (!response.Success)
            {
                throw new FilterException($"Plugin execution failed: {response.Error}");
            }

            // Receive processed image
            return await ReceiveImageDataAsync(response.ImageInfo);
        }
        finally
        {
            _commandLock.Release();
        }
    }

    private async Task SendCommandAsync(PluginCommand command)
    {
        var json = JsonSerializer.Serialize(command);
        var bytes = Encoding.UTF8.GetBytes(json);
        var lengthBytes = BitConverter.GetBytes(bytes.Length);

        await _commandPipe.WriteAsync(lengthBytes, 0, 4);
        await _commandPipe.WriteAsync(bytes, 0, bytes.Length);
        await _commandPipe.FlushAsync();
    }

    private async Task SendImageDataAsync(ImageData image)
    {
        // Send in chunks to avoid large allocations
        const int chunkSize = 1024 * 1024; // 1MB chunks
        var remaining = image.PixelData.Length;
        var offset = 0;

        while (remaining > 0)
        {
            var size = Math.Min(remaining, chunkSize);
            await _dataPipe.WriteAsync(image.PixelData, offset, size);

            offset += size;
            remaining -= size;
        }

        await _dataPipe.FlushAsync();
    }

    public void Dispose()
    {
        try
        {
            // Send shutdown command
            var shutdownCommand = new PluginCommand { Type = CommandType.Shutdown };
            SendCommandAsync(shutdownCommand).Wait(1000);
        }
        catch
        {
            // Ignore errors during shutdown
        }

        _commandPipe?.Dispose();
        _dataPipe?.Dispose();

        if (!_hostProcess.HasExited)
        {
            _hostProcess.Kill();
        }

        _hostProcess.Dispose();
    }
}
```

## 15.3 Plugin Discovery and Loading

Effective plugin discovery mechanisms enable applications to locate and catalog available plugins without compromising
startup performance. **Modern plugin systems implement multiple discovery strategies including file system monitoring,
registry-based catalogs, and network-based repositories**, allowing plugins to be added dynamically without application
restart. The discovery system must handle versioning, dependencies, and compatibility requirements while maintaining
security.

### File system monitoring and hot-reload capabilities

Dynamic plugin discovery through file system monitoring enables developers to add, update, or remove plugins while the
application is running. **The implementation must handle file locking issues common during development**, provide
appropriate debouncing to avoid repeated reloads, and ensure thread-safe updates to the plugin catalog.

```csharp
public class FileSystemPluginDiscovery : IPluginDiscovery, IDisposable
{
    private readonly List<string> _pluginDirectories;
    private readonly IPluginLoader _pluginLoader;
    private readonly ILogger<FileSystemPluginDiscovery> _logger;
    private readonly Dictionary<string, PluginInfo> _discoveredPlugins;
    private readonly List<FileSystemWatcher> _watchers;
    private readonly SemaphoreSlim _discoveryLock;
    private readonly PluginValidator _validator;

    public event EventHandler<PluginDiscoveryEventArgs> PluginDiscovered;
    public event EventHandler<PluginDiscoveryEventArgs> PluginUpdated;
    public event EventHandler<PluginDiscoveryEventArgs> PluginRemoved;

    public FileSystemPluginDiscovery(
        IEnumerable<string> pluginDirectories,
        IPluginLoader pluginLoader,
        ILogger<FileSystemPluginDiscovery> logger)
    {
        _pluginDirectories = pluginDirectories.ToList();
        _pluginLoader = pluginLoader;
        _logger = logger;
        _discoveredPlugins = new Dictionary<string, PluginInfo>();
        _watchers = new List<FileSystemWatcher>();
        _discoveryLock = new SemaphoreSlim(1);
        _validator = new PluginValidator();

        InitializeWatchers();
    }

    private void InitializeWatchers()
    {
        foreach (var directory in _pluginDirectories)
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogWarning($"Plugin directory does not exist: {directory}");
                Directory.CreateDirectory(directory);
            }

            var watcher = new FileSystemWatcher(directory)
            {
                NotifyFilter = NotifyFilters.FileName |
                              NotifyFilters.LastWrite |
                              NotifyFilters.Size,
                Filter = "*.dll",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            // Debounced event handlers
            var debouncer = new FileChangeDebouncer(TimeSpan.FromSeconds(1));

            watcher.Created += (s, e) => debouncer.Debounce(
                e.FullPath, () => OnPluginFileChanged(e.FullPath, ChangeType.Created));

            watcher.Changed += (s, e) => debouncer.Debounce(
                e.FullPath, () => OnPluginFileChanged(e.FullPath, ChangeType.Changed));

            watcher.Deleted += (s, e) => debouncer.Debounce(
                e.FullPath, () => OnPluginFileChanged(e.FullPath, ChangeType.Deleted));

            watcher.Renamed += (s, e) => debouncer.Debounce(
                e.FullPath, () => OnPluginFileRenamed(e.OldFullPath, e.FullPath));

            _watchers.Add(watcher);

            _logger.LogInformation($"Monitoring plugin directory: {directory}");
        }
    }

    public async Task<IEnumerable<PluginInfo>> DiscoverPluginsAsync()
    {
        await _discoveryLock.WaitAsync();
        try
        {
            var tasks = _pluginDirectories
                .SelectMany(dir => Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories))
                .Select(file => DiscoverPluginAsync(file));

            var results = await Task.WhenAll(tasks);

            // Update catalog
            _discoveredPlugins.Clear();
            foreach (var plugin in results.Where(p => p != null))
            {
                _discoveredPlugins[plugin.FilePath] = plugin;
            }

            _logger.LogInformation($"Discovered {_discoveredPlugins.Count} plugins");

            return _discoveredPlugins.Values;
        }
        finally
        {
            _discoveryLock.Release();
        }
    }

    private async Task<PluginInfo> DiscoverPluginAsync(string filePath)
    {
        try
        {
            // Quick validation before attempting to load
            if (!await _validator.QuickValidateAsync(filePath))
            {
                _logger.LogDebug($"Skipping invalid plugin file: {filePath}");
                return null;
            }

            // Extract metadata without loading
            var metadata = await ExtractPluginMetadataAsync(filePath);
            if (metadata == null)
            {
                return null;
            }

            // Check compatibility
            if (!IsCompatible(metadata))
            {
                _logger.LogWarning(
                    $"Plugin '{metadata.Name}' requires framework version {metadata.TargetFramework}");
                return null;
            }

            return new PluginInfo
            {
                FilePath = filePath,
                Name = metadata.Name,
                Version = metadata.Version,
                Description = metadata.Description,
                Author = metadata.Author,
                TargetFramework = metadata.TargetFramework,
                Dependencies = metadata.Dependencies,
                ExportedTypes = metadata.ExportedTypes,
                FileHash = await ComputeFileHashAsync(filePath),
                DiscoveryTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to discover plugin: {filePath}");
            return null;
        }
    }

    private async Task<PluginMetadata> ExtractPluginMetadataAsync(string filePath)
    {
        // Use MetadataLoadContext for lightweight metadata extraction
        var resolver = new PathAssemblyResolver(new[] { filePath });
        var mlc = new MetadataLoadContext(resolver);

        try
        {
            var assembly = mlc.LoadFromAssemblyPath(filePath);

            // Extract assembly attributes
            var metadata = new PluginMetadata
            {
                Name = assembly.GetName().Name,
                Version = assembly.GetName().Version,
                TargetFramework = GetTargetFramework(assembly)
            };

            // Extract custom attributes
            foreach (var attr in assembly.GetCustomAttributesData())
            {
                switch (attr.AttributeType.Name)
                {
                    case "AssemblyDescriptionAttribute":
                        metadata.Description = attr.ConstructorArguments[0].Value?.ToString();
                        break;
                    case "AssemblyCompanyAttribute":
                        metadata.Author = attr.ConstructorArguments[0].Value?.ToString();
                        break;
                    case "PluginDependencyAttribute":
                        metadata.Dependencies.Add(ParseDependency(attr));
                        break;
                }
            }

            // Find exported types
            foreach (var type in assembly.GetTypes())
            {
                if (type.GetInterfaces().Any(i => i.Name == nameof(IGraphicsFilter)))
                {
                    metadata.ExportedTypes.Add(new ExportedType
                    {
                        TypeName = type.FullName,
                        Interface = nameof(IGraphicsFilter),
                        Metadata = ExtractTypeMetadata(type)
                    });
                }
            }

            return metadata;
        }
        finally
        {
            mlc.Dispose();
        }
    }

    private async void OnPluginFileChanged(string filePath, ChangeType changeType)
    {
        await _discoveryLock.WaitAsync();
        try
        {
            switch (changeType)
            {
                case ChangeType.Created:
                case ChangeType.Changed:
                    await HandlePluginAddedOrUpdatedAsync(filePath);
                    break;

                case ChangeType.Deleted:
                    await HandlePluginRemovedAsync(filePath);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error handling plugin file change: {filePath}");
        }
        finally
        {
            _discoveryLock.Release();
        }
    }

    private async Task HandlePluginAddedOrUpdatedAsync(string filePath)
    {
        // Wait for file to be fully written
        if (!await WaitForFileAsync(filePath))
        {
            _logger.LogWarning($"Unable to access plugin file: {filePath}");
            return;
        }

        var pluginInfo = await DiscoverPluginAsync(filePath);
        if (pluginInfo == null)
        {
            return;
        }

        var isUpdate = _discoveredPlugins.ContainsKey(filePath);
        _discoveredPlugins[filePath] = pluginInfo;

        // Notify listeners
        var args = new PluginDiscoveryEventArgs { Plugin = pluginInfo };

        if (isUpdate)
        {
            _logger.LogInformation($"Plugin updated: {pluginInfo.Name} v{pluginInfo.Version}");
            PluginUpdated?.Invoke(this, args);
        }
        else
        {
            _logger.LogInformation($"Plugin discovered: {pluginInfo.Name} v{pluginInfo.Version}");
            PluginDiscovered?.Invoke(this, args);
        }
    }

    private async Task<bool> WaitForFileAsync(string filePath, int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                await Task.Delay(100 * (i + 1)); // Progressive delay
            }
        }

        return false;
    }

    public void Dispose()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _discoveryLock?.Dispose();
    }
}

// Debouncer to handle rapid file system events
public class FileChangeDebouncer
{
    private readonly TimeSpan _delay;
    private readonly Dictionary<string, Timer> _timers = new();
    private readonly object _lock = new();

    public FileChangeDebouncer(TimeSpan delay)
    {
        _delay = delay;
    }

    public void Debounce(string key, Action action)
    {
        lock (_lock)
        {
            if (_timers.TryGetValue(key, out var timer))
            {
                timer.Dispose();
            }

            _timers[key] = new Timer(_ =>
            {
                lock (_lock)
                {
                    _timers.Remove(key);
                }
                action();
            }, null, _delay, Timeout.InfiniteTimeSpan);
        }
    }
}
```

### Registry-based plugin catalogs

For enterprise deployments, registry-based catalogs provide centralized plugin management with version control and
dependency resolution. **This approach enables IT administrators to control plugin deployment across multiple machines
**, implement approval workflows, and ensure consistent plugin versions across an organization.

```csharp
public class RegistryPluginCatalog : IPluginCatalog
{
    private readonly string _registryEndpoint;
    private readonly HttpClient _httpClient;
    private readonly IPluginCache _cache;
    private readonly ILogger<RegistryPluginCatalog> _logger;
    private readonly PluginDependencyResolver _dependencyResolver;

    public RegistryPluginCatalog(
        string registryEndpoint,
        HttpClient httpClient,
        IPluginCache cache,
        ILogger<RegistryPluginCatalog> logger)
    {
        _registryEndpoint = registryEndpoint;
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _dependencyResolver = new PluginDependencyResolver();
    }

    public async Task<IEnumerable<CatalogEntry>> SearchPluginsAsync(
        string query = null,
        PluginCategory? category = null,
        Version minVersion = null)
    {
        var url = BuildSearchUrl(query, category, minVersion);

        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var results = JsonSerializer.Deserialize<SearchResults>(json);

            return results.Entries.Select(e => new CatalogEntry
            {
                Id = e.Id,
                Name = e.Name,
                Version = Version.Parse(e.Version),
                Description = e.Description,
                Author = e.Author,
                Category = Enum.Parse<PluginCategory>(e.Category),
                Downloads = e.Downloads,
                Rating = e.Rating,
                Dependencies = e.Dependencies,
                ReleaseNotes = e.ReleaseNotes,
                PublishedDate = e.PublishedDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search plugin catalog");
            throw new CatalogException("Unable to search plugin catalog", ex);
        }
    }

    public async Task<InstalledPlugin> InstallPluginAsync(
        string pluginId,
        Version version = null,
        bool includeDependencies = true)
    {
        // Check if already installed
        var installed = await _cache.GetInstalledPluginAsync(pluginId, version);
        if (installed != null)
        {
            _logger.LogInformation($"Plugin already installed: {pluginId} v{installed.Version}");
            return installed;
        }

        // Get plugin manifest
        var manifest = await GetPluginManifestAsync(pluginId, version);

        // Resolve dependencies
        if (includeDependencies)
        {
            var dependencies = await _dependencyResolver.ResolveAsync(manifest);

            // Install dependencies first
            foreach (var dep in dependencies)
            {
                await InstallPluginAsync(dep.Id, dep.Version, false);
            }
        }

        // Download plugin package
        var packagePath = await DownloadPluginPackageAsync(manifest);

        try
        {
            // Verify package integrity
            if (!await VerifyPackageIntegrityAsync(packagePath, manifest))
            {
                throw new SecurityException("Package integrity verification failed");
            }

            // Extract and install
            var installPath = await ExtractAndInstallAsync(packagePath, manifest);

            // Register in cache
            var installedPlugin = new InstalledPlugin
            {
                Id = manifest.Id,
                Name = manifest.Name,
                Version = manifest.Version,
                InstallPath = installPath,
                InstallDate = DateTime.UtcNow,
                Source = _registryEndpoint,
                Dependencies = manifest.Dependencies
            };

            await _cache.RegisterInstalledPluginAsync(installedPlugin);

            _logger.LogInformation($"Successfully installed plugin: {pluginId} v{manifest.Version}");

            return installedPlugin;
        }
        finally
        {
            // Cleanup temporary files
            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    private async Task<bool> VerifyPackageIntegrityAsync(
        string packagePath,
        PluginManifest manifest)
    {
        // Verify file hash
        var actualHash = await ComputeFileHashAsync(packagePath);
        if (actualHash != manifest.PackageHash)
        {
            _logger.LogWarning($"Package hash mismatch for {manifest.Id}");
            return false;
        }

        // Verify digital signature if present
        if (!string.IsNullOrEmpty(manifest.SignatureUrl))
        {
            var signature = await DownloadSignatureAsync(manifest.SignatureUrl);
            return VerifySignature(packagePath, signature, manifest.PublicKey);
        }

        return true;
    }

    public async Task<UpdateInfo[]> CheckForUpdatesAsync()
    {
        var installedPlugins = await _cache.GetAllInstalledPluginsAsync();
        var updateTasks = installedPlugins.Select(CheckPluginUpdateAsync);
        var updates = await Task.WhenAll(updateTasks);

        return updates.Where(u => u != null).ToArray();
    }

    private async Task<UpdateInfo> CheckPluginUpdateAsync(InstalledPlugin plugin)
    {
        try
        {
            var latestManifest = await GetPluginManifestAsync(plugin.Id, null);

            if (latestManifest.Version > plugin.Version)
            {
                return new UpdateInfo
                {
                    PluginId = plugin.Id,
                    CurrentVersion = plugin.Version,
                    LatestVersion = latestManifest.Version,
                    ReleaseNotes = latestManifest.ReleaseNotes,
                    IsCritical = latestManifest.IsCriticalUpdate,
                    PublishedDate = latestManifest.PublishedDate
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to check updates for plugin: {plugin.Id}");
        }

        return null;
    }
}

// Dependency resolver for complex plugin graphs
public class PluginDependencyResolver
{
    private readonly Dictionary<string, DependencyNode> _nodes = new();

    public async Task<IEnumerable<PluginDependency>> ResolveAsync(PluginManifest manifest)
    {
        _nodes.Clear();

        // Build dependency graph
        await BuildDependencyGraphAsync(manifest);

        // Topological sort to determine installation order
        var sorted = TopologicalSort();

        // Remove the root plugin itself
        return sorted
            .Where(n => n.Id != manifest.Id)
            .Select(n => new PluginDependency
            {
                Id = n.Id,
                Version = n.Version,
                IsRequired = n.IsRequired
            });
    }

    private async Task BuildDependencyGraphAsync(PluginManifest manifest)
    {
        var visited = new HashSet<string>();
        await BuildGraphRecursiveAsync(manifest, visited);
    }

    private async Task BuildGraphRecursiveAsync(
        PluginManifest manifest,
        HashSet<string> visited)
    {
        var key = $"{manifest.Id}:{manifest.Version}";
        if (visited.Contains(key))
        {
            return;
        }

        visited.Add(key);

        var node = new DependencyNode
        {
            Id = manifest.Id,
            Version = manifest.Version,
            IsRequired = true
        };

        _nodes[manifest.Id] = node;

        // Process each dependency
        foreach (var dep in manifest.Dependencies)
        {
            // Get dependency manifest
            var depManifest = await GetDependencyManifestAsync(dep);

            // Add edge
            node.Dependencies.Add(depManifest.Id);

            // Recurse
            await BuildGraphRecursiveAsync(depManifest, visited);
        }
    }

    private List<DependencyNode> TopologicalSort()
    {
        var sorted = new List<DependencyNode>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        foreach (var node in _nodes.Values)
        {
            if (!visited.Contains(node.Id))
            {
                TopologicalSortVisit(node, visited, visiting, sorted);
            }
        }

        return sorted;
    }

    private void TopologicalSortVisit(
        DependencyNode node,
        HashSet<string> visited,
        HashSet<string> visiting,
        List<DependencyNode> sorted)
    {
        if (visiting.Contains(node.Id))
        {
            throw new CircularDependencyException(
                $"Circular dependency detected involving plugin: {node.Id}");
        }

        if (visited.Contains(node.Id))
        {
            return;
        }

        visiting.Add(node.Id);

        foreach (var depId in node.Dependencies)
        {
            if (_nodes.TryGetValue(depId, out var depNode))
            {
                TopologicalSortVisit(depNode, visited, visiting, sorted);
            }
        }

        visiting.Remove(node.Id);
        visited.Add(node.Id);
        sorted.Add(node);
    }
}
```

### Network-based plugin repositories

Cloud-based plugin repositories enable dynamic plugin distribution with automatic updates, licensing verification, and
usage analytics. **The implementation must handle network failures gracefully**, implement appropriate caching to reduce
bandwidth usage, and support offline operation when repositories are unavailable.

```csharp
public class NetworkPluginRepository : IPluginRepository
{
    private readonly RepositoryConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly IPluginCache _cache;
    private readonly ILicenseManager _licenseManager;
    private readonly ILogger<NetworkPluginRepository> _logger;
    private readonly CircuitBreaker _circuitBreaker;

    public NetworkPluginRepository(
        RepositoryConfiguration config,
        HttpClient httpClient,
        IPluginCache cache,
        ILicenseManager licenseManager,
        ILogger<NetworkPluginRepository> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _cache = cache;
        _licenseManager = licenseManager;
        _logger = logger;

        // Circuit breaker for resilience
        _circuitBreaker = new CircuitBreaker(
            failureThreshold: 3,
            resetTimeout: TimeSpan.FromMinutes(1));
    }

    public async Task<PluginPackage> DownloadPluginAsync(
        string pluginId,
        Version version,
        IProgress<DownloadProgress> progress = null)
    {
        // Check cache first
        var cachedPackage = await _cache.GetPackageAsync(pluginId, version);
        if (cachedPackage != null && await ValidateCachedPackageAsync(cachedPackage))
        {
            _logger.LogDebug($"Using cached package for {pluginId} v{version}");
            return cachedPackage;
        }

        // Download from repository
        return await _circuitBreaker.ExecuteAsync(async () =>
        {
            var manifest = await GetManifestAsync(pluginId, version);

            // Verify license
            if (manifest.RequiresLicense)
            {
                var licenseValid = await _licenseManager.ValidateLicenseAsync(
                    pluginId, manifest.LicenseType);

                if (!licenseValid)
                {
                    throw new LicenseException($"Valid license required for {pluginId}");
                }
            }

            // Download package
            var packageUrl = BuildPackageUrl(manifest);
            var tempFile = Path.GetTempFileName();

            try
            {
                using (var response = await _httpClient.GetAsync(
                    packageUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1;
                    var downloadedBytes = 0L;

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(tempFile))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            downloadedBytes += bytesRead;

                            // Report progress
                            progress?.Report(new DownloadProgress
                            {
                                BytesDownloaded = downloadedBytes,
                                TotalBytes = totalBytes,
                                PercentComplete = totalBytes > 0
                                    ? (int)((downloadedBytes * 100) / totalBytes)
                                    : -1
                            });
                        }
                    }
                }

                // Verify download
                var package = new PluginPackage
                {
                    Id = pluginId,
                    Version = version,
                    FilePath = tempFile,
                    Manifest = manifest,
                    DownloadTime = DateTime.UtcNow
                };

                if (!await VerifyPackageAsync(package))
                {
                    throw new CorruptedPackageException($"Package verification failed for {pluginId}");
                }

                // Cache the package
                await _cache.StorePackageAsync(package);

                // Track analytics
                await SendAnalyticsAsync(new DownloadAnalytics
                {
                    PluginId = pluginId,
                    Version = version,
                    Timestamp = DateTime.UtcNow,
                    Success = true
                });

                return package;
            }
            catch (Exception ex)
            {
                // Cleanup on failure
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                // Track failure
                await SendAnalyticsAsync(new DownloadAnalytics
                {
                    PluginId = pluginId,
                    Version = version,
                    Timestamp = DateTime.UtcNow,
                    Success = false,
                    Error = ex.Message
                });

                throw;
            }
        });
    }

    public async Task<bool> CheckForUpdatesAsync(
        IEnumerable<InstalledPlugin> installedPlugins)
    {
        try
        {
            // Batch update check for efficiency
            var updateRequest = new UpdateCheckRequest
            {
                Plugins = installedPlugins.Select(p => new PluginVersion
                {
                    Id = p.Id,
                    Version = p.Version.ToString()
                }).ToList()
            };

            var json = JsonSerializer.Serialize(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_config.BaseUrl}/api/updates/check", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Update check failed: {response.StatusCode}");
                return false;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var updates = JsonSerializer.Deserialize<UpdateCheckResponse>(responseJson);

            // Process updates
            foreach (var update in updates.Updates)
            {
                await _cache.StoreUpdateInfoAsync(new CachedUpdateInfo
                {
                    PluginId = update.PluginId,
                    CurrentVersion = Version.Parse(update.CurrentVersion),
                    LatestVersion = Version.Parse(update.LatestVersion),
                    ReleaseNotes = update.ReleaseNotes,
                    IsCritical = update.IsCritical,
                    CheckTime = DateTime.UtcNow
                });
            }

            return updates.Updates.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check for updates");

            // Fall back to cached update info
            return false;
        }
    }

    private async Task<bool> VerifyPackageAsync(PluginPackage package)
    {
        // Verify file integrity
        var actualHash = await HashingHelper.ComputeSHA256Async(package.FilePath);
        if (actualHash != package.Manifest.PackageHash)
        {
            _logger.LogWarning($"Hash mismatch for {package.Id}: " +
                $"expected {package.Manifest.PackageHash}, got {actualHash}");
            return false;
        }

        // Verify signature if required
        if (_config.RequireSignedPackages)
        {
            var signatureValid = await VerifyPackageSignatureAsync(
                package.FilePath,
                package.Manifest.Signature);

            if (!signatureValid)
            {
                _logger.LogWarning($"Invalid signature for {package.Id}");
                return false;
            }
        }

        // Scan for malware if configured
        if (_config.EnableMalwareScanning)
        {
            var scanResult = await ScanForMalwareAsync(package.FilePath);
            if (!scanResult.IsClean)
            {
                _logger.LogWarning($"Malware detected in {package.Id}: {scanResult.ThreatName}");
                return false;
            }
        }

        return true;
    }

    // Resilient analytics reporting
    private async Task SendAnalyticsAsync(DownloadAnalytics analytics)
    {
        try
        {
            // Fire and forget with retry
            _ = Task.Run(async () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(analytics);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await _httpClient.PostAsync(
                            $"{_config.BaseUrl}/api/analytics/download", content);

                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        // Ignore analytics failures
                    }

                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                }
            });
        }
        catch
        {
            // Never fail due to analytics
        }
    }
}
```

## 15.4 API Design for Extensions

Designing extensible APIs requires careful balance between flexibility and stability. **The API must provide sufficient
power for plugins to implement sophisticated functionality while maintaining backward compatibility across versions**.
Well-designed extension APIs use interface segregation, provide clear contracts with semantic versioning, and include
comprehensive documentation and examples.

### Interface segregation and plugin contracts

The Interface Segregation Principle becomes critical when designing plugin APIs, as overly broad interfaces force
plugins to implement unnecessary methods and create versioning challenges. **A well-designed plugin system provides
focused interfaces for specific capabilities**, allowing plugins to implement only the functionality they need while the
host can query for supported interfaces.

```csharp
// Core interfaces following ISP
public interface IGraphicsPlugin
{
    string Id { get; }
    string Name { get; }
    Version Version { get; }
    IPluginCapabilities Capabilities { get; }
}

public interface IPluginCapabilities
{
    bool SupportsInterface<T>() where T : class;
    T GetInterface<T>() where T : class;
    IEnumerable<Type> SupportedInterfaces { get; }
}

// Segregated capability interfaces
public interface IImageFilter : IGraphicsPlugin
{
    Task<FilterResult> ProcessAsync(IImageData input, IFilterParameters parameters);
    IFilterMetadata Metadata { get; }
}

public interface IBatchProcessor
{
    Task<BatchResult> ProcessBatchAsync(
        IEnumerable<IImageData> inputs,
        IBatchParameters parameters,
        IProgress<BatchProgress> progress,
        CancellationToken cancellationToken);

    int MaxBatchSize { get; }
    bool SupportsParallelProcessing { get; }
}

public interface IConfigurableFilter
{
    IFilterConfiguration DefaultConfiguration { get; }
    IFilterConfiguration DeserializeConfiguration(string json);
    ValidationResult ValidateConfiguration(IFilterConfiguration configuration);
    IConfigurationUI CreateConfigurationUI();
}

public interface IPreviewableFilter
{
    Task<IImageData> GeneratePreviewAsync(
        IImageData input,
        IFilterParameters parameters,
        PreviewQuality quality);

    bool SupportsRealTimePreview { get; }
    TimeSpan EstimatedPreviewTime(IImageData input, PreviewQuality quality);
}

public interface IGpuAcceleratedFilter
{
    bool IsGpuAvailable();
    GpuRequirements GetGpuRequirements();
    Task<FilterResult> ProcessOnGpuAsync(
        IImageData input,
        IFilterParameters parameters,
        IGpuContext context);
}

// Plugin implementation with multiple capabilities
[Export(typeof(IImageFilter))]
public class AdvancedBlurFilter : IImageFilter, IBatchProcessor,
    IConfigurableFilter, IPreviewableFilter, IGpuAcceleratedFilter
{
    public string Id => "com.example.blur.advanced";
    public string Name => "Advanced Blur";
    public Version Version => new Version(2, 1, 0);

    private readonly PluginCapabilities _capabilities;

    public AdvancedBlurFilter()
    {
        _capabilities = new PluginCapabilities(this);
    }

    public IPluginCapabilities Capabilities => _capabilities;

    public IFilterMetadata Metadata => new FilterMetadata
    {
        Category = FilterCategory.Blur,
        Description = "GPU-accelerated advanced blur with multiple algorithms",
        SupportedFormats = new[] { ImageFormat.RGBA32, ImageFormat.RGB24 },
        PerformanceHints = new PerformanceHints
        {
            IsMemoryIntensive = true,
            EstimatedMemoryUsage = (width, height) => width * height * 4 * 3, // 3 buffers
            SupportsInPlace = false
        }
    };

    // IImageFilter implementation
    public async Task<FilterResult> ProcessAsync(
        IImageData input,
        IFilterParameters parameters)
    {
        var config = parameters.GetConfiguration<BlurConfiguration>();

        // Choose optimal processing path
        if (IsGpuAvailable() && config.PreferGpuAcceleration)
        {
            var gpuContext = parameters.GetService<IGpuContext>();
            return await ProcessOnGpuAsync(input, parameters, gpuContext);
        }

        return await ProcessOnCpuAsync(input, config);
    }

    // IBatchProcessor implementation
    public async Task<BatchResult> ProcessBatchAsync(
        IEnumerable<IImageData> inputs,
        IBatchParameters parameters,
        IProgress<BatchProgress> progress,
        CancellationToken cancellationToken)
    {
        var results = new List<FilterResult>();
        var config = parameters.GetConfiguration<BlurConfiguration>();

        // Process in parallel if supported
        if (SupportsParallelProcessing && config.AllowParallelProcessing)
        {
            await Parallel.ForEachAsync(
                inputs.Select((input, index) => (input, index)),
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                },
                async (item, ct) =>
                {
                    var result = await ProcessAsync(item.input, parameters);

                    lock (results)
                    {
                        results.Add(result);
                    }

                    progress?.Report(new BatchProgress
                    {
                        ProcessedCount = results.Count,
                        TotalCount = inputs.Count(),
                        CurrentItem = item.index
                    });
                });
        }
        else
        {
            // Sequential processing
            foreach (var (input, index) in inputs.Select((img, i) => (img, i)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await ProcessAsync(input, parameters);
                results.Add(result);

                progress?.Report(new BatchProgress
                {
                    ProcessedCount = results.Count,
                    TotalCount = inputs.Count(),
                    CurrentItem = index
                });
            }
        }

        return new BatchResult
        {
            Results = results,
            TotalProcessingTime = results.Sum(r => r.ProcessingTime.TotalMilliseconds)
        };
    }

    public int MaxBatchSize => 100;
    public bool SupportsParallelProcessing => true;

    // IConfigurableFilter implementation
    public IFilterConfiguration DefaultConfiguration => new BlurConfiguration
    {
        Algorithm = BlurAlgorithm.Gaussian,
        Radius = 5.0f,
        Sigma = 1.5f,
        Quality = ProcessingQuality.High,
        PreferGpuAcceleration = true,
        AllowParallelProcessing = true
    };

    public IFilterConfiguration DeserializeConfiguration(string json)
    {
        return JsonSerializer.Deserialize<BlurConfiguration>(json);
    }

    public ValidationResult ValidateConfiguration(IFilterConfiguration configuration)
    {
        var result = new ValidationResult();

        if (configuration is not BlurConfiguration config)
        {
            result.AddError("Invalid configuration type");
            return result;
        }

        if (config.Radius < 0.1f || config.Radius > 100f)
        {
            result.AddError("Radius must be between 0.1 and 100");
        }

        if (config.Sigma < 0.01f || config.Sigma > 50f)
        {
            result.AddError("Sigma must be between 0.01 and 50");
        }

        return result;
    }

    public IConfigurationUI CreateConfigurationUI()
    {
        return new BlurConfigurationUI();
    }

    // IPreviewableFilter implementation
    public async Task<IImageData> GeneratePreviewAsync(
        IImageData input,
        IFilterParameters parameters,
        PreviewQuality quality)
    {
        // Downsample for faster preview
        var scale = quality switch
        {
            PreviewQuality.Fast => 0.25f,
            PreviewQuality.Balanced => 0.5f,
            PreviewQuality.Quality => 0.75f,
            _ => 1.0f
        };

        var previewSize = new Size(
            (int)(input.Width * scale),
            (int)(input.Height * scale));

        var downsampled = await input.ResizeAsync(previewSize);
        var result = await ProcessAsync(downsampled, parameters);

        // Upscale back to original size
        return await result.Output.ResizeAsync(new Size(input.Width, input.Height));
    }

    public bool SupportsRealTimePreview => true;

    public TimeSpan EstimatedPreviewTime(IImageData input, PreviewQuality quality)
    {
        var scale = quality switch
        {
            PreviewQuality.Fast => 0.25f,
            PreviewQuality.Balanced => 0.5f,
            PreviewQuality.Quality => 0.75f,
            _ => 1.0f
        };

        var pixels = input.Width * input.Height * scale * scale;
        var millisecondsPerMegapixel = IsGpuAvailable() ? 5 : 20;
        var estimatedMs = (pixels / 1_000_000) * millisecondsPerMegapixel;

        return TimeSpan.FromMilliseconds(estimatedMs);
    }

    // IGpuAcceleratedFilter implementation
    public bool IsGpuAvailable()
    {
        return GpuManager.Instance.HasCompatibleGpu();
    }

    public GpuRequirements GetGpuRequirements()
    {
        return new GpuRequirements
        {
            MinimumShaderModel = "5.0",
            RequiredMemoryMB = 256,
            RequiredFeatures = new[]
            {
                GpuFeature.ComputeShaders,
                GpuFeature.TextureArrays,
                GpuFeature.UnorderedAccessViews
            }
        };
    }

    public async Task<FilterResult> ProcessOnGpuAsync(
        IImageData input,
        IFilterParameters parameters,
        IGpuContext context)
    {
        var config = parameters.GetConfiguration<BlurConfiguration>();

        // Compile shader for specific algorithm
        var shader = config.Algorithm switch
        {
            BlurAlgorithm.Gaussian => await context.CompileShaderAsync(GaussianBlurShader),
            BlurAlgorithm.Box => await context.CompileShaderAsync(BoxBlurShader),
            BlurAlgorithm.Motion => await context.CompileShaderAsync(MotionBlurShader),
            _ => throw new NotSupportedException($"Algorithm {config.Algorithm} not supported on GPU")
        };

        // Execute on GPU
        using var inputBuffer = await context.CreateBufferAsync(input);
        using var outputBuffer = await context.CreateBufferAsync(input.Width, input.Height);

        await context.DispatchAsync(shader, new ShaderParameters
        {
            ["radius"] = config.Radius,
            ["sigma"] = config.Sigma,
            ["width"] = input.Width,
            ["height"] = input.Height
        }, inputBuffer, outputBuffer);

        var outputData = await outputBuffer.ReadAsync();

        return new FilterResult
        {
            Output = new GpuImageData(outputData, input.Width, input.Height, input.Format),
            ProcessingTime = context.LastDispatchTime,
            ProcessingMethod = ProcessingMethod.Gpu
        };
    }
}
```

### Versioning strategies and backward compatibility

Maintaining backward compatibility while evolving plugin APIs requires careful versioning strategies and explicit
compatibility policies. **Semantic versioning provides clear communication about breaking changes**, while adapter
patterns and interface evolution techniques allow APIs to grow without breaking existing plugins.

```csharp
// Versioned interfaces with compatibility attributes
[PluginInterface("ImageFilter", Version = "1.0.0")]
public interface IImageFilterV1
{
    Task<IImageData> ProcessAsync(IImageData input, Dictionary<string, object> parameters);
}

[PluginInterface("ImageFilter", Version = "2.0.0")]
[CompatibleWith(typeof(IImageFilterV1))]
public interface IImageFilterV2 : IImageFilterV1
{
    new Task<FilterResult> ProcessAsync(IImageData input, IFilterParameters parameters);
    IFilterMetadata GetMetadata();
}

[PluginInterface("ImageFilter", Version = "3.0.0")]
[CompatibleWith(typeof(IImageFilterV2))]
public interface IImageFilterV3 : IImageFilterV2
{
    Task<FilterResult> ProcessAsync(
        IImageData input,
        IFilterParameters parameters,
        IProcessingContext context);

    bool SupportsStreaming { get; }
    Task<IAsyncEnumerable<IImageData>> ProcessStreamAsync(
        IAsyncEnumerable<IImageData> inputs,
        IFilterParameters parameters);
}

// Compatibility adapter for legacy plugins
public class FilterCompatibilityAdapter : IImageFilterV3
{
    private readonly object _innerFilter;
    private readonly Version _filterVersion;

    public FilterCompatibilityAdapter(object innerFilter, Version filterVersion)
    {
        _innerFilter = innerFilter;
        _filterVersion = filterVersion;
    }

    public async Task<FilterResult> ProcessAsync(
        IImageData input,
        IFilterParameters parameters,
        IProcessingContext context)
    {
        // Route to appropriate version
        switch (_filterVersion.Major)
        {
            case 1:
                return await ProcessV1Async(input, parameters);
            case 2:
                return await ProcessV2Async(input, parameters);
            case 3:
                return await ((IImageFilterV3)_innerFilter).ProcessAsync(
                    input, parameters, context);
            default:
                throw new NotSupportedException(
                    $"Filter version {_filterVersion} not supported");
        }
    }

    private async Task<FilterResult> ProcessV1Async(
        IImageData input,
        IFilterParameters parameters)
    {
        var v1Filter = (IImageFilterV1)_innerFilter;

        // Convert parameters to V1 format
        var v1Params = ConvertParametersToV1(parameters);

        var stopwatch = Stopwatch.StartNew();
        var output = await v1Filter.ProcessAsync(input, v1Params);

        return new FilterResult
        {
            Output = output,
            ProcessingTime = stopwatch.Elapsed,
            ProcessingMethod = ProcessingMethod.Unknown
        };
    }

    private Dictionary<string, object> ConvertParametersToV1(IFilterParameters parameters)
    {
        var result = new Dictionary<string, object>();

        foreach (var param in parameters.GetAll())
        {
            result[param.Key] = param.Value;
        }

        return result;
    }

    // Delegating implementations for other methods...
}

// API evolution helper
public class ApiEvolution
{
    private readonly Dictionary<Type, List<ApiChange>> _changes = new();

    public void RegisterApiChange<TInterface>(ApiChange change)
    {
        if (!_changes.TryGetValue(typeof(TInterface), out var changes))
        {
            changes = new List<ApiChange>();
            _changes[typeof(TInterface)] = changes;
        }

        changes.Add(change);
    }

    public IEnumerable<ApiChange> GetChanges<TInterface>(Version fromVersion, Version toVersion)
    {
        if (!_changes.TryGetValue(typeof(TInterface), out var changes))
        {
            return Enumerable.Empty<ApiChange>();
        }

        return changes
            .Where(c => c.IntroducedIn > fromVersion && c.IntroducedIn <= toVersion)
            .OrderBy(c => c.IntroducedIn);
    }

    public MigrationGuide GenerateMigrationGuide<TInterface>(
        Version fromVersion,
        Version toVersion)
    {
        var changes = GetChanges<TInterface>(fromVersion, toVersion);
        var guide = new MigrationGuide
        {
            InterfaceName = typeof(TInterface).Name,
            FromVersion = fromVersion,
            ToVersion = toVersion
        };

        foreach (var change in changes)
        {
            guide.Steps.Add(new MigrationStep
            {
                ChangeType = change.Type,
                Description = change.Description,
                CodeBefore = change.ExampleBefore,
                CodeAfter = change.ExampleAfter,
                AutomationAvailable = change.CanAutomate
            });
        }

        return guide;
    }
}

// Capability negotiation for progressive enhancement
public class CapabilityNegotiator
{
    private readonly Dictionary<string, CapabilityDefinition> _capabilities = new();

    public void RegisterCapability(CapabilityDefinition capability)
    {
        _capabilities[capability.Id] = capability;
    }

    public NegotiationResult Negotiate(
        IPluginCapabilities pluginCapabilities,
        IHostCapabilities hostCapabilities)
    {
        var result = new NegotiationResult();

        foreach (var capability in _capabilities.Values)
        {
            var pluginSupports = pluginCapabilities.SupportsCapability(capability.Id);
            var hostSupports = hostCapabilities.SupportsCapability(capability.Id);

            if (pluginSupports && hostSupports)
            {
                var negotiated = NegotiateCapabilityLevel(
                    capability,
                    pluginCapabilities.GetCapabilityLevel(capability.Id),
                    hostCapabilities.GetCapabilityLevel(capability.Id));

                result.EnabledCapabilities.Add(negotiated);
            }
            else if (pluginSupports && !hostSupports && capability.HasFallback)
            {
                result.FallbackCapabilities.Add(new FallbackCapability
                {
                    Capability = capability,
                    Reason = "Host does not support capability",
                    FallbackStrategy = capability.FallbackStrategy
                });
            }
        }

        return result;
    }

    private NegotiatedCapability NegotiateCapabilityLevel(
        CapabilityDefinition definition,
        int pluginLevel,
        int hostLevel)
    {
        var negotiatedLevel = Math.Min(pluginLevel, hostLevel);

        return new NegotiatedCapability
        {
            Id = definition.Id,
            Name = definition.Name,
            NegotiatedLevel = negotiatedLevel,
            Features = definition.GetFeaturesForLevel(negotiatedLevel)
        };
    }
}
```

### Documentation and testing frameworks for plugin developers

Comprehensive documentation and testing support significantly reduces plugin development friction and improves quality.
**Automated API documentation generation ensures accuracy**, while plugin testing frameworks provide sandboxed
environments for validation. Sample plugins demonstrate best practices and common patterns.

```csharp
// Plugin testing framework
public class PluginTestHarness
{
    private readonly TestHost _host;
    private readonly ILogger<PluginTestHarness> _logger;

    public PluginTestHarness(ILogger<PluginTestHarness> logger)
    {
        _logger = logger;
        _host = new TestHost();
    }

    public async Task<TestReport> RunTestSuiteAsync(IGraphicsFilter filter)
    {
        var report = new TestReport
        {
            PluginName = filter.Name,
            PluginVersion = filter.Version,
            TestStartTime = DateTime.UtcNow
        };

        // Run test categories
        await RunFunctionalTestsAsync(filter, report);
        await RunPerformanceTestsAsync(filter, report);
        await RunCompatibilityTestsAsync(filter, report);
        await RunSecurityTestsAsync(filter, report);
        await RunStressTestsAsync(filter, report);

        report.TestEndTime = DateTime.UtcNow;
        report.GenerateSummary();

        return report;
    }

    private async Task RunFunctionalTestsAsync(IGraphicsFilter filter, TestReport report)
    {
        var testSuite = new FunctionalTestSuite();

        // Basic functionality tests
        var basicTests = new[]
        {
            testSuite.TestNullInput(filter),
            testSuite.TestEmptyImage(filter),
            testSuite.TestVariousFormats(filter),
            testSuite.TestVariousSizes(filter),
            testSuite.TestDefaultConfiguration(filter),
            testSuite.TestConfigurationValidation(filter)
        };

        foreach (var test in basicTests)
        {
            var result = await RunTestAsync(test);
            report.AddResult(TestCategory.Functional, result);
        }

        // Edge case tests
        if (filter.Metadata.SupportedFormats.Contains(ImageFormat.RGBA32))
        {
            var alphaTests = new[]
            {
                testSuite.TestTransparency(filter),
                testSuite.TestPremultipliedAlpha(filter),
                testSuite.TestAlphaBlending(filter)
            };

            foreach (var test in alphaTests)
            {
                var result = await RunTestAsync(test);
                report.AddResult(TestCategory.Functional, result);
            }
        }
    }

    private async Task RunPerformanceTestsAsync(IGraphicsFilter filter, TestReport report)
    {
        var perfSuite = new PerformanceTestSuite();

        // Benchmark different image sizes
        var sizes = new[]
        {
            (256, 256),
            (1024, 1024),
            (4096, 4096),
            (8192, 8192)
        };

        foreach (var (width, height) in sizes)
        {
            var test = perfSuite.CreateBenchmarkTest(filter, width, height);
            var result = await RunTestAsync(test);

            result.Metrics["ThroughputMPixelsPerSec"] =
                (width * height / 1_000_000.0) / result.ExecutionTime.TotalSeconds;

            report.AddResult(TestCategory.Performance, result);
        }

        // Memory usage tests
        var memoryTest = perfSuite.CreateMemoryUsageTest(filter);
        var memoryResult = await RunTestAsync(memoryTest);
        report.AddResult(TestCategory.Performance, memoryResult);
    }

    private async Task RunSecurityTestsAsync(IGraphicsFilter filter, TestReport report)
    {
        var securitySuite = new SecurityTestSuite();

        // Input validation tests
        var maliciousInputTests = new[]
        {
            securitySuite.TestOversizedImage(filter, int.MaxValue, int.MaxValue),
            securitySuite.TestMalformedConfiguration(filter),
            securitySuite.TestResourceExhaustion(filter),
            securitySuite.TestConcurrentAccess(filter),
            securitySuite.TestPathTraversal(filter)
        };

        foreach (var test in maliciousInputTests)
        {
            var result = await RunTestAsync(test);
            report.AddResult(TestCategory.Security, result);
        }
    }

    private async Task<TestResult> RunTestAsync(IPluginTest test)
    {
        var result = new TestResult
        {
            TestName = test.Name,
            TestDescription = test.Description
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await test.ExecuteAsync(_host);
            result.Status = TestStatus.Passed;
        }
        catch (TestAssertionException ex)
        {
            result.Status = TestStatus.Failed;
            result.FailureReason = ex.Message;
            result.StackTrace = ex.StackTrace;
        }
        catch (Exception ex)
        {
            result.Status = TestStatus.Error;
            result.FailureReason = ex.Message;
            result.Exception = ex;
        }
        finally
        {
            result.ExecutionTime = stopwatch.Elapsed;
        }

        return result;
    }
}

// Documentation generator for plugin APIs
public class PluginDocumentationGenerator
{
    private readonly IPluginCatalog _catalog;
    private readonly ApiIntrospector _introspector;

    public async Task GenerateDocumentationAsync(string outputPath)
    {
        var plugins = await _catalog.GetAllPluginsAsync();

        foreach (var plugin in plugins)
        {
            await GeneratePluginDocumentationAsync(plugin, outputPath);
        }

        // Generate index and cross-references
        await GenerateIndexAsync(plugins, outputPath);
        await GenerateCrossReferencesAsync(plugins, outputPath);
    }

    private async Task GeneratePluginDocumentationAsync(
        IGraphicsPlugin plugin,
        string outputPath)
    {
        var docBuilder = new DocumentationBuilder();

        // Header
        docBuilder.AddHeader(1, $"{plugin.Name} Plugin Documentation");
        docBuilder.AddMetadata("Version", plugin.Version.ToString());
        docBuilder.AddMetadata("Plugin ID", plugin.Id);

        // Capabilities
        docBuilder.AddSection("Capabilities", () =>
        {
            foreach (var capability in plugin.Capabilities.SupportedInterfaces)
            {
                docBuilder.AddSubsection(capability.Name, () =>
                {
                    GenerateInterfaceDocumentation(capability, docBuilder);
                });
            }
        });

        // Configuration
        if (plugin.Capabilities.SupportsInterface<IConfigurableFilter>())
        {
            var configurable = plugin.Capabilities.GetInterface<IConfigurableFilter>();
            docBuilder.AddSection("Configuration", () =>
            {
                GenerateConfigurationDocumentation(
                    configurable.DefaultConfiguration,
                    docBuilder);
            });
        }

        // Code examples
        docBuilder.AddSection("Examples", () =>
        {
            GenerateCodeExamples(plugin, docBuilder);
        });

        // Performance characteristics
        if (plugin is IImageFilter filter)
        {
            docBuilder.AddSection("Performance", () =>
            {
                GeneratePerformanceDocumentation(filter.Metadata, docBuilder);
            });
        }

        // Save documentation
        var pluginDocPath = Path.Combine(outputPath, $"{plugin.Id}.md");
        await File.WriteAllTextAsync(pluginDocPath, docBuilder.Build());
    }

    private void GenerateCodeExamples(IGraphicsPlugin plugin, DocumentationBuilder builder)
    {
        // Basic usage example
        builder.AddCodeBlock("csharp", $@"
// Basic usage
var filter = await pluginHost.GetFilterAsync(""{plugin.Name}"");
var result = await filter.ProcessAsync(inputImage, new FilterParameters
{{
    Configuration = filter.CreateDefaultConfiguration()
}});
");

        // Advanced examples based on capabilities
        if (plugin.Capabilities.SupportsInterface<IBatchProcessor>())
        {
            builder.AddCodeBlock("csharp", $@"
// Batch processing
var batchProcessor = filter.Capabilities.GetInterface<IBatchProcessor>();
var results = await batchProcessor.ProcessBatchAsync(
    images,
    new BatchParameters {{ Configuration = config }},
    new Progress<BatchProgress>(p => Console.WriteLine($""Processed {{p.ProcessedCount}}/{{p.TotalCount}}"")),
    cancellationToken);
");
        }
    }
}

// Sample plugin demonstrating best practices
[Export(typeof(IImageFilter))]
[PluginMetadata("SamplePlugin", "1.0.0", "Demonstrates plugin best practices")]
public class SamplePlugin : IImageFilter, IConfigurableFilter, IDisposable
{
    private readonly ILogger<SamplePlugin> _logger;
    private readonly object _resourceLock = new();
    private bool _disposed;

    [ImportingConstructor]
    public SamplePlugin(ILogger<SamplePlugin> logger)
    {
        _logger = logger;
        _logger.LogInformation($"Initializing {Name} v{Version}");
    }

    public string Id => "com.example.sample";
    public string Name => "Sample Plugin";
    public Version Version => new Version(1, 0, 0);

    public IPluginCapabilities Capabilities => new PluginCapabilities(this);

    public IFilterMetadata Metadata => new FilterMetadata
    {
        Category = FilterCategory.Enhancement,
        Description = "A sample plugin demonstrating best practices",
        SupportedFormats = new[] { ImageFormat.RGBA32, ImageFormat.RGB24 },
        Tags = new[] { "sample", "educational", "reference" }
    };

    public async Task<FilterResult> ProcessAsync(
        IImageData input,
        IFilterParameters parameters)
    {
        ThrowIfDisposed();

        _logger.LogDebug($"Processing image: {input.Width}x{input.Height}");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validate input
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Get configuration
            var config = parameters.GetConfiguration<SampleConfiguration>()
                ?? DefaultConfiguration as SampleConfiguration;

            // Process image
            var output = await ProcessImageAsync(input, config);

            return new FilterResult
            {
                Output = output,
                ProcessingTime = stopwatch.Elapsed,
                ProcessingMethod = ProcessingMethod.Cpu,
                Metadata = new Dictionary<string, object>
                {
                    ["ProcessedPixels"] = input.Width * input.Height,
                    ["Configuration"] = config
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed");
            throw new FilterException($"Processing failed: {ex.Message}", ex);
        }
    }

    private async Task<IImageData> ProcessImageAsync(
        IImageData input,
        SampleConfiguration config)
    {
        // Demonstrate async processing with cancellation support
        return await Task.Run(() =>
        {
            lock (_resourceLock)
            {
                // Simulate processing
                var output = input.Clone();

                // Apply simple brightness adjustment as example
                output.ProcessPixels((ref Rgba32 pixel) =>
                {
                    pixel.R = ClampByte(pixel.R + config.BrightnessAdjustment);
                    pixel.G = ClampByte(pixel.G + config.BrightnessAdjustment);
                    pixel.B = ClampByte(pixel.B + config.BrightnessAdjustment);
                });

                return output;
            }
        });
    }

    private static byte ClampByte(int value)
    {
        return (byte)Math.Clamp(value, 0, 255);
    }

    public IFilterConfiguration DefaultConfiguration => new SampleConfiguration
    {
        BrightnessAdjustment = 10,
        PreserveAlpha = true
    };

    public IFilterConfiguration DeserializeConfiguration(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SampleConfiguration>(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize configuration");
            return DefaultConfiguration;
        }
    }

    public ValidationResult ValidateConfiguration(IFilterConfiguration configuration)
    {
        var result = new ValidationResult();

        if (configuration is not SampleConfiguration config)
        {
            result.AddError("Configuration must be of type SampleConfiguration");
            return result;
        }

        if (config.BrightnessAdjustment < -255 || config.BrightnessAdjustment > 255)
        {
            result.AddError("BrightnessAdjustment must be between -255 and 255");
        }

        return result;
    }

    public IConfigurationUI CreateConfigurationUI()
    {
        // Return null for headless operation
        // Implement for UI support
        return null;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SamplePlugin));
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _logger.LogInformation($"Disposing {Name}");

        // Cleanup resources
        lock (_resourceLock)
        {
            // Release any unmanaged resources
            _disposed = true;
        }
    }
}

[Serializable]
public class SampleConfiguration : IFilterConfiguration
{
    public int BrightnessAdjustment { get; set; }
    public bool PreserveAlpha { get; set; }

    public object Clone()
    {
        return new SampleConfiguration
        {
            BrightnessAdjustment = BrightnessAdjustment,
            PreserveAlpha = PreserveAlpha
        };
    }
}
```

## Summary

Building a robust plugin architecture for graphics processing applications requires careful attention to multiple
architectural concerns. The Managed Extensibility Framework provides a solid foundation for plugin discovery and
composition, while modern .NET features like AssemblyLoadContext enable proper isolation and versioning. Security must
be considered at every level, from assembly loading through API design, with defense-in-depth strategies protecting both
the host application and user data.

The key to successful plugin architecture lies in balancing power with safety, flexibility with stability, and ease of
use with proper constraints. Interface segregation enables plugins to implement only needed functionality, while
capability negotiation allows progressive enhancement based on available features. Comprehensive testing frameworks and
documentation generators reduce development friction and improve plugin quality.

Modern plugin systems must handle dynamic discovery through multiple channels, support hot-reloading for development
productivity, and provide clear migration paths as APIs evolve. By following the patterns and practices outlined in this
chapter, developers can create extensible graphics processing systems that grow with user needs while maintaining the
performance and reliability expected of professional software.
