using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace Kentico.Xperience.Typesense.Admin;

internal class TypesenseModuleInstaller
{
    private readonly IInfoProvider<ResourceInfo> resourceProvider;

    public TypesenseModuleInstaller(IInfoProvider<ResourceInfo> resourceProvider) => this.resourceProvider = resourceProvider;

    public void Install()
    {
        var resource = resourceProvider.Get("CMS.Integration.Typesense")
            ?? new ResourceInfo();

        InitializeResource(resource);
        InstallTypesenseItemInfo(resource);
        InstallTypesenseLanguageInfo(resource);
        InstallTypesenseCollectionPathItemInfo(resource);
        InstallTypesenseContentTypeItemInfo(resource);
    }

    public ResourceInfo InitializeResource(ResourceInfo resource)
    {
        resource.ResourceDisplayName = "Kentico Integration - Typesense";

        // Prefix ResourceName with "CMS" to prevent C# class generation
        // Classes are already available through the library itself
        resource.ResourceName = "CMS.Integration.Typesense";
        resource.ResourceDescription = "Kentico Typesense custom data";
        resource.ResourceIsInDevelopment = false;
        if (resource.HasChanged)
        {
            resourceProvider.Set(resource);
        }

        return resource;
    }

    public void InstallTypesenseItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(TypesenseCollectionItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(TypesenseCollectionItemInfo.OBJECT_TYPE);

        info.ClassName = TypesenseCollectionItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = TypesenseCollectionItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Collection Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemcollectionName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemChannelName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemStrategyName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemRebuildHook),
            AllowEmpty = true,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallTypesenseCollectionPathItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(TypesenseIncludedPathItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(TypesenseIncludedPathItemInfo.OBJECT_TYPE);

        info.ClassName = TypesenseIncludedPathItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = TypesenseIncludedPathItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Path Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemAliasPath),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = TypesenseCollectionItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallTypesenseLanguageInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(TypesenseCollectionLanguageItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(TypesenseCollectionLanguageItemInfo.OBJECT_TYPE);

        info.ClassName = TypesenseCollectionLanguageItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = TypesenseCollectionLanguageItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Collectioned Language Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
            Enabled = true
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = TypesenseCollectionItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    public void InstallTypesenseContentTypeItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(TypesenseContentTypeItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(TypesenseContentTypeItemInfo.OBJECT_TYPE);

        info.ClassName = TypesenseContentTypeItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = TypesenseContentTypeItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Type Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemContentTypeName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = "text",
            Enabled = true,
            IsUnique = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemIncludedPathItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = TypesenseIncludedPathItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemGuid),
            Enabled = true,
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "guid",
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = "integer",
            ReferenceToObjectType = TypesenseCollectionItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
