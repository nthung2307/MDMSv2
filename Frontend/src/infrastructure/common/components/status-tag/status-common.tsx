import Constants from '@/core/application/common/constants';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common';

export const renderStatus = (record: any) => {
  switch (record.status) {
    case Constants.UserStatusConfig.Active.value:
      return <StatusTagCommon status={Constants.UserStatusConfig.Active.label} />;
    case Constants.UserStatusConfig.Inactive.value:
      return (
        <StatusTagCommon status={Constants.UserStatusConfig.Inactive.label} bgColor='#0000004D' />
      );
    default:
      return '';
  }
};
