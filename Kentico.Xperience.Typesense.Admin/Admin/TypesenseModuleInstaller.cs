using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;

namespace Kentico.Xperience.Typesense.Xperience;

public class TypesenseModuleInstaller
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
        InstallIndexQueueItemInfo(resource);
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
        var info = DataClassInfoProvider.GetDataClassInfo(TypesenseIndexItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(TypesenseIndexItemInfo.OBJECT_TYPE);

        info.ClassName = TypesenseIndexItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = TypesenseIndexItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Index Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(TypesenseIndexItemInfo.TypesenseCollectionItemId));

        var formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexItemInfo.TypesenseCollectionItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Guid,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexItemInfo.TypesenseCollectionItemcollectionName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = FieldDataType.Text,
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexItemInfo.TypesenseCollectionItemChannelName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = FieldDataType.Text,
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexItemInfo.TypesenseCollectionItemStrategyName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = FieldDataType.Text,
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexItemInfo.TypesenseCollectionItemRebuildHook),
            AllowEmpty = true,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = FieldDataType.Text,
            Enabled = true
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            try
            {
                DataClassInfoProvider.SetDataClassInfo(info);
            }
            catch (Exception ex)
            {
                //TODO : Investigate the issue here
            }
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
            DataType = FieldDataType.Guid,
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
            DataType = FieldDataType.Text,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Integer,
            ReferenceToObjectType = TypesenseIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            try
            {
                DataClassInfoProvider.SetDataClassInfo(info);
            }
            catch (Exception ex)
            {
                //TODO : Investigate the issue here
            }
        }
    }

    public void InstallTypesenseLanguageInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(TypesenseIndexLanguageItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(TypesenseIndexLanguageItemInfo.OBJECT_TYPE);

        info.ClassName = TypesenseIndexLanguageItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = TypesenseIndexLanguageItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Collectioned Language Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = FieldDataType.Text,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Guid,
            Enabled = true
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Integer,
            ReferenceToObjectType = TypesenseIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            try
            {
                DataClassInfoProvider.SetDataClassInfo(info);
            }
            catch (Exception ex)
            {
                //TODO : Investigate the issue here
            }
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
            DataType = FieldDataType.Text,
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
            DataType = FieldDataType.Integer,
            ReferenceToObjectType = TypesenseIncludedPathItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Integer,
            ReferenceToObjectType = TypesenseIndexItemInfo.OBJECT_TYPE,
            ReferenceType = ObjectDependencyEnum.Required,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemGuid),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Guid,
            Enabled = true
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            try
            {
                DataClassInfoProvider.SetDataClassInfo(info);
            }
            catch (Exception ex)
            {
                //TODO : Investigate the issue here
            }
        }
    }

    public void InstallIndexQueueItemInfo(ResourceInfo resource)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(IndexQueueItemInfo.OBJECT_TYPE) ?? DataClassInfo.New(IndexQueueItemInfo.OBJECT_TYPE);

        info.ClassName = IndexQueueItemInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = IndexQueueItemInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Typesense Sql Queue Item";
        info.ClassType = ClassType.OTHER;
        info.ClassResourceID = resource.ResourceID;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(IndexQueueItemInfo.IndexQueueItemID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(IndexQueueItemInfo.TaskType),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            Size = 100,
            DataType = FieldDataType.Integer,
            Enabled = true,
            IsUnique = false
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(IndexQueueItemInfo.CollectionName),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Text,
            Enabled = true,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(IndexQueueItemInfo.CollectionEvent),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.LongText,
            Enabled = true,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(IndexQueueItemInfo.EnqueuedAt),
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.DateTime,
            Enabled = true,
        };

        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(IndexQueueItemInfo.IndexQueueItemGuid),
            Enabled = true,
            AllowEmpty = false,
            Visible = true,
            Precision = 0,
            DataType = FieldDataType.Guid,
        };

        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            try
            {
                DataClassInfoProvider.SetDataClassInfo(info);
            }
            catch (Exception ex)
            {
                //TODO : Investigate the issue here
            }
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
