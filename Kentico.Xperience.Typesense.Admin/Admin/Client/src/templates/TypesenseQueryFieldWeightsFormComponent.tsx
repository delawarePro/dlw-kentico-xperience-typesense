import { type FormComponentProps } from '@kentico/xperience-admin-base';
import {
  type ActionCell,
  Button,
  ButtonType,
  CellType,
  ColumnContentType,
  Input,
  Stack,
  type StringCell,
  Table,
  type TableAction,
  type TableCell,
  type TableColumn,
  type TableRow,
} from '@kentico/xperience-admin-components';
import React, { useEffect, useState } from 'react';
import { JSX } from 'react/jsx-runtime';

export interface TypesenseQueryFieldWeight {
  fieldName: string;
  weight: number;
  identifier?: string | null;
}

export interface TypesenseQueryFieldWeightsComponentClientProperties
  extends FormComponentProps {
  value: TypesenseQueryFieldWeight[];
}

export const TypesenseQueryFieldWeightsFormComponent = (
  props: TypesenseQueryFieldWeightsComponentClientProperties,
): JSX.Element => {
  const [rows, setRows] = useState<TableRow[]>([]);
  const [showFieldEdit, setShowFieldEdit] = useState<boolean>(false);
  const [fieldName, setFieldName] = useState<string>('');
  const [fieldWeight, setFieldWeight] = useState<number>(1);
  const [editedIdentifier, setEditedIdentifier] = useState<string>('');
  const [showAddNewField, setShowAddNewField] = useState<boolean>(true);

  const prepareRows = (
    fieldWeights: TypesenseQueryFieldWeight[],
  ): TableRow[] => {
    if (fieldWeights === undefined) {
      return [];
    }

    const getCells = (fieldWeight: TypesenseQueryFieldWeight): TableCell[] => {
      const fieldNameVal: string = fieldWeight.fieldName?.toString() ?? '';
      const weightVal: string = fieldWeight.weight?.toString() ?? '1';

      if (fieldWeight.fieldName === null || fieldWeight.fieldName === '') {
        return [];
      }

      const fieldNameCell: StringCell = {
        type: CellType.String,
        value: fieldNameVal,
      };

      const weightCell: StringCell = {
        type: CellType.String,
        value: weightVal,
      };

      const deleteAction: TableAction = {
        label: 'delete',
        icon: 'xp-bin',
        disabled: false,
        destructive: true,
      };

      const deleteField: () => Promise<void> = async () => {
        await new Promise(() => {
          props.value = props.value.filter((x) => x.fieldName !== fieldNameVal);

          if (props.onChange !== null && props.onChange !== undefined) {
            props.onChange(props.value);
          }

          setRows(prepareRows(props.value));
          setShowFieldEdit(false);
          setFieldName('');
          setFieldWeight(1);
          setEditedIdentifier('');
          setShowAddNewField(true);
        });
      };

      const deleteFieldCell: ActionCell = {
        actions: [deleteAction],
        type: CellType.Action,
        onInvokeAction: deleteField,
      };

      const cells: TableCell[] = [fieldNameCell, weightCell, deleteFieldCell];
      return cells;
    };

    return fieldWeights.map((fieldWeight) => {
      const row: TableRow = {
        identifier: fieldWeight.fieldName,
        cells: getCells(fieldWeight),
        disabled: false,
      };
      return row;
    });
  };

  useEffect(() => {
    if (props.value === null || props.value === undefined) {
      props.value = [];
    }
    if (props.onChange !== null && props.onChange !== undefined) {
      props.onChange(props.value);
    }
    setRows(() => prepareRows(props.value));
  }, [props?.value]);

  const prepareColumns = (): TableColumn[] => {
    const columns: TableColumn[] = [];

    const fieldNameColumn: TableColumn = {
      name: 'Field Name',
      visible: true,
      contentType: ColumnContentType.Text,
      caption: 'Field Name',
      minWidth: 0,
      maxWidth: 1000,
      sortable: true,
      searchable: true,
    };

    const weightColumn: TableColumn = {
      name: 'Weight',
      visible: true,
      contentType: ColumnContentType.Text,
      caption: 'Weight',
      minWidth: 0,
      maxWidth: 200,
      sortable: true,
      searchable: false,
    };

    const actionColumn: TableColumn = {
      name: 'Actions',
      visible: true,
      contentType: ColumnContentType.Action,
      caption: 'Actions',
      minWidth: 0,
      maxWidth: 200,
      sortable: false,
      searchable: false,
    };

    columns.push(fieldNameColumn);
    columns.push(weightColumn);
    columns.push(actionColumn);
    return columns;
  };

  const showFieldItems = (identifier: unknown): void => {
    let rowIndex = -1;
    for (let i = 0; i < rows.length; i++) {
      if ((rows[i].identifier as string) === (identifier as string)) {
        rowIndex = i;
      }
    }
    const row = rows[rowIndex];

    setFieldName((row.cells[0] as StringCell).value);
    setFieldWeight(parseInt((row.cells[1] as StringCell).value) || 1);

    if (!showFieldEdit) {
      setEditedIdentifier((row.cells[0] as StringCell).value);
    } else {
      setEditedIdentifier('');
    }

    setShowFieldEdit(!showFieldEdit);
    setShowAddNewField(!showAddNewField);
  };

  const handleFieldNameChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void => {
    setFieldName(event.target.value);
  };
  const handleWeightChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void => {
    const value = parseInt(event.target.value, 10);
    if (!isNaN(value) && value >= 1 && value <= 100) {
      setFieldWeight(value);
    }
  };

  const saveField = (): void => {
    if (editedIdentifier === '') {
      // Adding new field
      if (
        !rows.some((x) => {
          return x.identifier === fieldName;
        })
      ) {
        if (fieldName === '') {
          alert('Field name cannot be empty');
        } else {
          const newFieldWeight: TypesenseQueryFieldWeight = {
            fieldName: fieldName,
            weight: fieldWeight,
            identifier: null,
          };
          props.value.push(newFieldWeight);
          setRows(prepareRows(props.value));
        }
      } else {
        alert('This field already exists!');
      }
    } else {
      // Editing existing field
      const rowIndex = rows.findIndex((x) => {
        return x.identifier === editedIdentifier;
      });

      if (rowIndex === -1) {
        alert('Invalid edit');
      } else {
        const propFieldIndex = props.value.findIndex(
          (p) => p.fieldName === editedIdentifier,
        );

        if (propFieldIndex !== -1) {
          const updatedFieldWeight: TypesenseQueryFieldWeight = {
            fieldName: fieldName,
            weight: fieldWeight,
            identifier: props.value[propFieldIndex].identifier,
          };

          props.value[propFieldIndex] = updatedFieldWeight;

          const newRows = [...rows];
          const editedRow = newRows[rowIndex];
          const fieldNameCellInNewRow = editedRow.cells[0] as StringCell;
          const weightCellInNewRow = editedRow.cells[1] as StringCell;
          fieldNameCellInNewRow.value = fieldName;
          weightCellInNewRow.value = fieldWeight.toString();
          editedRow.identifier = fieldName;

          newRows[rowIndex] = editedRow;
          setRows(newRows);
        }
      }
    }

    setEditedIdentifier('');
    setShowFieldEdit(false);
    setShowAddNewField(true);
    setFieldName('');
    setFieldWeight(1);
  };

  const addNewField = (): void => {
    setShowFieldEdit(true);
    setFieldName('');
    setFieldWeight(1);
    setEditedIdentifier('');
    setShowAddNewField(false);
  };

  return (
    <Stack>
      <Table
        columns={prepareColumns()}
        rows={rows}
        onRowClick={showFieldItems}
      />
      {showFieldEdit && (
        <div>
          <br></br>
          <Input
            label="Field Name"
            value={fieldName}
            onChange={handleFieldNameChange}
            explanationText="Enter the field name to search"
          />
          <br></br>
          <Input
            type="number"
            label="Weight"
            value={fieldWeight.toString()}
            onChange={handleWeightChange}
            explanationText="Higher weights give more importance to matches in this field (1-100)"
            min="1"
            max="100"
          />
          <br></br>
          <Button
            type={ButtonType.Button}
            label="Save Field"
            onClick={saveField}
          ></Button>
        </div>
      )}
      <br></br>
      {showAddNewField && (
        <Button
          type={ButtonType.Button}
          label="Add new field"
          onClick={addNewField}
        ></Button>
      )}
    </Stack>
  );
};
