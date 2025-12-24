using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Localization;
using Volo.Abp.Validation;
using cima.Localization;

namespace cima.Blazor.Client.Services;

public class ExceptionMessageParser
{
    private readonly IStringLocalizer<cimaResource> _l;

    public ExceptionMessageParser(IStringLocalizer<cimaResource> l)
    {
        _l = l;
    }

    public string Parse(Exception ex)
    {
        if (ex is AbpValidationException validationEx)
        {
            var sb = new StringBuilder();
            foreach (var error in validationEx.ValidationErrors)
            {
                sb.AppendLine(error.ErrorMessage);
            }
            return sb.ToString().Trim();
        }

        // Add parsing for other known ABP exceptions here (e.g. UserFriendlyException)

        return _l["Common:Error"];
    }
}
