import React from 'react';

interface ChartSwitcherProps {
  chartType: string;
  chartList: Array<{ type: string; chartComponent: React.FC<any> }>;
  filterData: any;
  dataForm: { customerCode: string; monitorPoint: string };
}

const ChartSwitcher: React.FC<ChartSwitcherProps> = ({
  chartType,
  chartList,
  filterData,
  dataForm,
}) => {
  const ChartComponent = chartList.find((chart) => chart.type === chartType)?.chartComponent;

  if (!ChartComponent) {
    return <div>No chart available for the selected type</div>;
  }

  return (
    <>
      {filterData && dataForm.customerCode && dataForm.monitorPoint ? (
        <ChartComponent filterData={filterData} dataForm={dataForm} />
      ) : null}
    </>
  );
};

export default ChartSwitcher;
