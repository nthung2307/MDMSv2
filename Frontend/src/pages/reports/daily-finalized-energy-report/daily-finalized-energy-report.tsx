import React from 'react';

import HalfHourFinalizedEnergyReport from '@/pages/reports/half-hour-finalized-energy-report/half-hour-finalized-energy-report';

const DailyFinalizedEnergyReport: React.FC = () => {
  return <HalfHourFinalizedEnergyReport defaultReportType='monthly' />;
};

export default DailyFinalizedEnergyReport;
