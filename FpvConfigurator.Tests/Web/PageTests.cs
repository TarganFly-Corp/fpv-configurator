using Bunit;
using FpvConfigurator.Web.Components.Pages;
using FpvConfigurator.Web.Components.Shared;
using FpvConfigurator.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace FpvConfigurator.Tests.Web;

/// <summary>Сторінки Blazor у bUnit: рендер без браузера, клік і введення як у користувача (TC-95…TC-98, TC-105).</summary>
public sealed class PageTests : IDisposable
{
    private readonly WebTestData _data = new();
    private readonly BunitContext _ctx = new();

    public PageTests() => _data.AddServices(_ctx.Services);

    public void Dispose()
    {
        _ctx.Dispose();
        _data.Dispose();
    }

    /// <summary>TC-95: фільтр каталогу за категорією і наявністю (ФВ-17). Рам 4, у наявності 3 (fr-4 немає).</summary>
    [Fact]
    public void Catalog_FilterFramesInStock_ShowsOnlyAvailable()
    {
        var cut = _ctx.Render<CatalogPage>();

        cut.Find("select").Change("Frame");
        Assert.Equal(4, cut.FindAll(".part-card").Count);

        cut.Find("input[type=checkbox]").Change(true);
        var ids = cut.FindAll(".part-card .muted").Select(e => e.TextContent).ToList();

        Assert.Equal(3, cut.FindAll(".part-card").Count);
        Assert.DoesNotContain(ids, text => text.Contains("fr-4"));
    }

    /// <summary>TC-96: форма нової деталі показує помилку біля кожного неправильного поля (ФВ-22) і нічого не додає.</summary>
    [Fact]
    public void NewPart_InvalidValues_ErrorsNearFields()
    {
        _ctx.Services.GetRequiredService<NavigationManager>().NavigateTo("parts/new?category=Receiver");
        var cut = _ctx.Render<NewPartPage>();

        cut.Find("#part-id").Change("rx-1");
        cut.Find("#part-name").Change("RP1");
        cut.Find("#part-manufacturer").Change("RadioMaster");
        cut.Find("#part-price").Change("600");
        cut.Find("#part-weight").Change("0");
        cut.Find("#field-frequencyGhz input").Change("5.8");
        cut.Find("#field-antennas input").Change("2");
        cut.Find("#save-part").Click();

        Assert.Contains("вже є в каталозі", cut.Find("#part-id + .field-error").TextContent);
        Assert.Contains("більшим за 0", cut.Find("#part-weight + .field-error").TextContent);
        Assert.Contains("обов'язковий", cut.Find("#field-protocol .field-error").TextContent);
        Assert.Contains("не може бути більшим за 2", cut.Find("#field-frequencyGhz .field-error").TextContent);
        Assert.Empty(cut.FindAll("#field-antennas .field-error"));
        Assert.Equal(0, _ctx.Services.GetRequiredService<UserCatalog>().UserPartCount);
    }

    /// <summary>TC-97: конструктор збірки показує проблеми сумісності, ціну і вагу (ФВ-19).</summary>
    [Fact]
    public void Build_IncompatibleStack_ShowsProblemAndTotals()
    {
        _ctx.Services.GetRequiredService<UserCatalog>(); // як у Program.cs: каталог завантажується до першої сторінки
        var session = _ctx.Services.GetRequiredService<BuildSession>();
        session.Add("fr-1");   // рама 30.5x30.5, 1200 грн, 110 г
        session.Add("fc-2");   // контролер 20x20, 1300 грн, 5 г

        var cut = _ctx.Render<BuildPage>();

        Assert.Contains("Проблем сумісності: 1", cut.Find("#build-status").TextContent);
        Assert.Contains("Кріплення стеку", cut.Find("#problems").TextContent);
        Assert.Equal("2 500 грн", cut.Find("#total-price").TextContent.Replace(' ', ' '));
        Assert.Equal("115 г", cut.Find("#total-weight").TextContent);

        cut.Find("#slot-FlightController .icon-btn").Click();
        Assert.Contains("Збірка сумісна", cut.Find("#build-status").TextContent);
    }

    /// <summary>TC-98: редактор категорій не приймає двох параметрів з однаковим ключем, навіть у різному регістрі (ФВ-21).</summary>
    [Theory]
    [InlineData("axes", "axes")]
    [InlineData("axes", "AXES")]
    public void CategoryEditor_DuplicateParameterKey_Rejected(string first, string second)
    {
        var cut = _ctx.Render<CategoryEditor>();

        cut.Find("#category-key").Change("Gimbal");
        cut.Find("#category-name").Change("Підвіс камери");
        cut.Find("#param-0 .param-key").Change(first);
        cut.Find("#param-0 .param-name").Change("Осей");
        cut.Find("button:contains('Додати параметр')").Click();
        cut.Find("#param-1 .param-key").Change(second);
        cut.Find("#param-1 .param-name").Change("Осі");
        cut.Find("#save-category").Click();

        Assert.Contains("описано двічі", cut.Find(".field-error").TextContent);
        Assert.False(_ctx.Services.GetRequiredService<UserCatalog>().Registry.TryGet("Gimbal", out _));
    }

    /// <summary>
    /// TC-105: редактор правил (ФВ-28). Порожня назва дає помилку біля поля.
    /// Правило-приклад з'являється у списку, і збірка ESC 35 A з моторами 45 A стає несумісною.
    /// </summary>
    [Fact]
    public void Rules_AddExample_BuildGetsProblem()
    {
        var session = _ctx.Services.GetRequiredService<BuildSession>();
        _ctx.Services.GetRequiredService<UserRules>();
        session.Add("esc-2");  // 35 A
        session.Add("mt-2");   // 45 A
        Assert.True(session.Report.IsCompatible);

        var cut = _ctx.Render<RulesPage>();
        Assert.Equal(4, cut.FindAll("#builtin-rules li").Count);

        cut.Find("#example-rule").Click();
        cut.Find("#rule-name").Change("");
        cut.Find("#save-rule").Click();
        Assert.Contains("назву", cut.Find("#rule-name + .field-error").TextContent);

        cut.Find("#rule-name").Change("ESC тягне мотори");
        Assert.Equal("Регулятор обертів (ESC) · Струм ≥ Мотор · Макс. струм", cut.Find("#rule-preview").TextContent);
        cut.Find("#save-rule").Click();

        Assert.Contains("ESC тягне мотори", cut.Find("#user-rules").TextContent);
        var problem = Assert.Single(session.Report.Problems);
        Assert.StartsWith("«ESC тягне мотори»", problem.Message);
    }
}