# Testing Strategy - CIMA Blazor Application

## Overview
Comprehensive testing implementation for CIMA including E2E tests, UI component tests, admin tests, and performance audits.

---

## Test Structure

### 1. E2E Tests (Playwright)
**Location:** `test/cima.Blazor.E2ETests/`

#### Public Site Tests (`PublicSiteTests.cs`)
- ? HomePage_Should_LoadSuccessfully
- ? HomePage_Should_DisplayHeroSection
- ? HomePage_Should_DisplayFeaturedProperties
- ? PropertiesPage_Should_LoadSuccessfully
- ? PropertiesPage_Should_DisplaySearchFilters
- ? PropertiesPage_Should_FilterByPropertyType
- ? PropertyDetail_Should_LoadWhenCardClicked
- ? PropertyDetail_Should_DisplayImageGallery
- ? PropertyDetail_Should_DisplayContactForm
- ? ContactForm_Should_ValidateRequiredFields
- ? PortfolioPage_Should_LoadSuccessfully
- ? PortfolioPage_Should_DisplayArchitectProjects
- ? Navigation_Should_WorkBetweenPages
- ? LanguageSwitch_Should_ChangeContentLanguage
- ? MobileView_Should_DisplayCorrectly

#### Admin Panel Tests (`AdminPanelTests.cs`)
- ? AdminDashboard_Should_RequireAuthentication
- ? AdminDashboard_Should_LoadAfterMockLogin
- ? AdminListings_Should_DisplayListingsGrid
- ? AdminListings_Should_HaveCreateButton
- ? AdminDashboard_Should_DisplayStatistics

---

### 2. UI Component Tests (bUnit)
**Location:** `test/cima.Blazor.UITests/`

#### ListingCard Tests (`Components/ListingCardTests.cs`)
- ? ListingCard_Should_RenderWithValidData
- ? ListingCard_Should_DisplayPrice
- ? ListingCard_Should_DisplayPropertyDetails
- ? ListingCard_Should_HandleNullListing

#### HeroSection Tests (`Components/HeroSectionTests.cs`)
- ? HeroSection_Should_Render
- ? HeroSection_Should_ContainCallToAction
- ? HeroSection_Should_DisplayMainHeading

#### ImageGallery Tests (`Components/ImageGalleryTests.cs`)
- ? ImageGallery_Should_RenderWithImages
- ? ImageGallery_Should_DisplayImagesInOrder
- ? ImageGallery_Should_HandleEmptyImageList
- ? ImageGallery_Should_SetAltTextOnImages

#### ContactForm Tests (`Components/ContactFormTests.cs`)
- ? ContactForm_Should_Render
- ? ContactForm_Should_ContainRequiredFields
- ? ContactForm_Should_HaveSubmitButton
- ? ContactForm_Should_ShowValidationForEmptyName

#### PropertySearchFilters Tests (`Components/PropertySearchFiltersTests.cs`)
- ? PropertySearchFilters_Should_Render
- ? PropertySearchFilters_Should_ContainFilterInputs
- ? PropertySearchFilters_Should_HaveSearchButton

---

### 3. Performance Audits (Lighthouse)
**Location:** `etc/scripts/run-performance-audit.ps1`

#### Metrics Tracked
- ? Performance Score
- ? Accessibility Score
- ? Best Practices Score
- ?? SEO Score

#### Performance Metrics
- First Contentful Paint (FCP)
- Largest Contentful Paint (LCP)
- Total Blocking Time (TBT)
- Cumulative Layout Shift (CLS)
- Speed Index

---

## Running Tests

### Prerequisites

1. **Install Playwright browsers:**
```bash
dotnet build test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj
pwsh test/cima.Blazor.E2ETests/bin/Debug/net9.0/playwright.ps1 install
```

2. **Install Lighthouse (for performance audits):**
```bash
npm install -g lighthouse
```

### Run All Tests
```bash
.\etc\scripts\run-all-tests.ps1
```

### Run Specific Test Suites

**Domain Tests:**
```bash
dotnet test test/cima.Domain.Tests/cima.Domain.Tests.csproj
```

**Application Tests:**
```bash
dotnet test test/cima.Application.Tests/cima.Application.Tests.csproj
```

**UI Component Tests:**
```bash
dotnet test test/cima.Blazor.UITests/cima.Blazor.UITests.csproj
```

**E2E Tests (requires app running):**
```bash
# Terminal 1: Start the app
dotnet run --project src/cima.Blazor/cima.Blazor.csproj

# Terminal 2: Run E2E tests
dotnet test test/cima.Blazor.E2ETests/cima.Blazor.E2ETests.csproj
```

### Run Performance Audit
```bash
# Start the app first
dotnet run --project src/cima.Blazor/cima.Blazor.csproj

# In another terminal:
.\etc\scripts\run-performance-audit.ps1
```

---

## Test Configuration

### E2E Test Configuration
**File:** `test/cima.Blazor.E2ETests/playwright.config.json`

```json
{
  "baseUrl": "http://localhost:5000",
  "headless": true,
  "timeout": 30000,
  "retries": 2
}
```

### Environment Variables
- `E2E_BASE_URL`: Override base URL for E2E tests (default: http://localhost:5000)

---

## Continuous Integration

### GitHub Actions Workflow
The test suite integrates with existing CI/CD:

**Workflow:** `.github/workflows/test-domain.yml`
- Runs on every push and PR
- Executes Domain and Application tests
- Can be extended to include UI and E2E tests

---

## Test Coverage Goals

| Test Type | Target Coverage | Current Status |
|-----------|----------------|----------------|
| Domain | 80%+ | ? Achieved |
| Application | 75%+ | ? Achieved |
| UI Components | 60%+ | ? Implemented |
| E2E Critical Paths | 100% | ? Implemented |

---

## Best Practices

### 1. E2E Tests
- ? Use `PlaywrightTestBase` for consistent setup
- ? Wait for `NetworkIdle` before assertions
- ? Use explicit waits instead of hardcoded delays
- ? Take screenshots on failures
- ? Test critical user paths first

### 2. UI Component Tests
- ? Test component in isolation
- ? Mock external dependencies
- ? Test both happy path and edge cases
- ? Verify accessibility attributes

### 3. Performance Tests
- ? Run against production builds
- ? Test on different network conditions
- ? Monitor Core Web Vitals
- ? Set performance budgets

---

## Known Limitations

### E2E Tests
- ? Admin tests require authentication setup
- ? Some tests skip if app is not running
- ? Playwright browsers need to be installed

### UI Tests
- ? Some complex interactions may need mocking
- ? Authentication-dependent components need setup

---

## Troubleshooting

### Playwright Installation Issues
```bash
# Install browsers manually
pwsh test/cima.Blazor.E2ETests/bin/Debug/net9.0/playwright.ps1 install chromium
```

### E2E Tests Fail with Connection Errors
- Ensure the application is running on http://localhost:5000
- Check firewall settings
- Verify no port conflicts

### bUnit Tests Fail
- Ensure all component dependencies are registered in test setup
- Check for missing service mocks

---

## Future Enhancements

### Planned Improvements
- [ ] Add visual regression testing
- [ ] Implement load testing with k6
- [ ] Add mutation testing
- [ ] Set up test data factories
- [ ] Add contract testing for APIs
- [ ] Implement accessibility testing automation
- [ ] Add cross-browser E2E tests

### Performance Monitoring
- [ ] Integrate with Application Insights
- [ ] Set up real user monitoring (RUM)
- [ ] Create performance dashboards
- [ ] Implement synthetic monitoring

---

## Resources

### Tools Used
- **xUnit**: Unit testing framework
- **Playwright**: E2E testing framework
- **bUnit**: Blazor component testing
- **Lighthouse**: Performance auditing
- **Moq**: Mocking framework (in existing tests)

### Documentation
- [Playwright for .NET](https://playwright.dev/dotnet/)
- [bUnit Documentation](https://bunit.dev/)
- [Lighthouse Documentation](https://developers.google.com/web/tools/lighthouse)
- [xUnit Documentation](https://xunit.net/)

---

## Quick Reference

### Common Commands
```bash
# Run all tests
.\etc\scripts\run-all-tests.ps1

# Run only unit tests
.\etc\scripts\run-all-tests.ps1 -SkipE2E -SkipUI

# Run only E2E tests
.\etc\scripts\run-all-tests.ps1 -SkipUnit -SkipUI

# Run performance audit
.\etc\scripts\run-performance-audit.ps1

# Install Playwright
pwsh test/cima.Blazor.E2ETests/bin/Debug/net9.0/playwright.ps1 install
```

---

**Last Updated:** 2025-01-28  
**Test Projects Created:** 2  
**Total Tests Implemented:** 30+  
**Coverage:** Domain, Application, UI, E2E, Performance
