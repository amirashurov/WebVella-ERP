﻿using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using WebVella.Erp.Api;
using WebVella.Erp.Api.Models;
using WebVella.Erp.Exceptions;
using WebVella.Erp.Plugins.SDK.Services;
using WebVella.Erp.Web;
using WebVella.Erp.Web.Models;

namespace WebVella.Erp.Plugins.SDK.Pages.Tools
{
	public class CodeGenModel : BaseErpPageModel
	{
		public CodeGenModel([FromServices]ErpRequestContext reqCtx) { ErpRequestContext = reqCtx; }

		public List<GridColumn> Columns { get; set; } = new List<GridColumn>() { 
			new GridColumn() { Label = "Element", Name = "", Width = "1%" },
			new GridColumn() { Label = "Change", Name = "", Width = "1%" },
			new GridColumn() { Label = "Name", Name = "", Width = "1%" },
			new GridColumn() { Label = "description", Name = "", Width = "90%" }
		};

		public List<MetaChangeModel> Changes { get; set; } = new List<MetaChangeModel>();

		public string Code { get; set; } = string.Empty;

		public bool ShowResults { get; set; } = false;

		[BindProperty]
		public string ConnectionString { get; set; }

		[BindProperty]
		public bool IncludeMeta { get; set; } = true;

		[BindProperty]
		public bool IncludeEntityRelations { get; set; } = true;

		[BindProperty]
		public bool IncludeUserRoles { get; set; } = true;

		[BindProperty]
		public bool IncludeApplications { get; set; } = true;

		[BindProperty]
		public List<string> IncludeRecordsEntityIdList { get; set; } = new List<string>();

		public List<SelectOption> EntitySelectOptions { get; set; } = new List<SelectOption>();

		public void OnGet()
		{
			Init();
			var entities = new EntityManager().ReadEntities().Object;
			entities = entities.OrderBy(x => x.Name).ToList();
			foreach (var entity in entities)
			{
				EntitySelectOptions.Add(new SelectOption(entity.Id.ToString(), entity.Name));
			}
		}

		public IActionResult OnPost()
		{
			if (!ModelState.IsValid) throw new Exception("Antiforgery check failed.");
			Init();

			try
			{
				ValidationException valEx = new ValidationException();
				if( string.IsNullOrWhiteSpace(ConnectionString) )
				{
					valEx.AddError("ConnectionString", "Original database connection string is required.");
					ShowResults = false;
					throw valEx;
				}

				var conString = ConnectionString;
				if( !ConnectionString.Contains(";"))
				{
					NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(ErpSettings.ConnectionString);
					builder.Database = ConnectionString;
					conString = builder.ToString();
				}	
				
				var cgService = new CodeGenService();
				var result = cgService.EvaluateMetaChanges(conString, IncludeMeta, IncludeEntityRelations, IncludeUserRoles, IncludeApplications);
				Code = result.Code;
				Changes = result.Changes;
				ShowResults = true;
			}
			catch (ValidationException valEx)
			{
				Validation.Message = valEx.Message;
				Validation.Errors.AddRange(valEx.Errors);
				ShowResults = false;
			}
			catch (Exception ex)
			{
				Validation.Message = ex.Message;
				Validation.AddError("", ex.Message );
				ShowResults = false;
			}

			return Page();
		}
	}
}