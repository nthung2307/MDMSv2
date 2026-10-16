import { Dropdown, MenuProps, TableProps, Tooltip } from 'antd';
import { BsThreeDots } from 'react-icons/bs';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import TableCommon from '@/infrastructure/common/components/table/table-common';

interface Props {
  dataSource: any;
  loading?: boolean;
}

const TableSummary = (props: Props) => {
  const { dataSource, loading } = props;

  const renderValue = (val: number, quanlity: boolean, unit?: any) => {
    if (!val && !quanlity) return `????`;

    return (
      <>
        {`${val} ${unit ? unit : ''} `}
        <span className='text-[#FF0000] font-bold'>{quanlity ? '' : '!'}</span>
      </>
    );
  };

  const columns: TableProps['columns'] = [
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPointCode',
      fixed: true,
      width: '150px',
      className: 'antd-action-table',
      render: (record) => {
        return record ? <span>{record}</span> : <span>N/A</span>;
      },
    },
    // {
    //   title: 'Mã KH',
    //   dataIndex: 'customerCode',
    //   className: 'antd-action-table',
    //   fixed: true,
    //   width: 150,
    //   render: (record) => {
    //     return record ? <span>{record}</span> : <span>N/A</span>;
    //   },
    // },
    {
      title: 'Tên KH',
      dataIndex: 'customerName',
      width: 200,
      render: (record) => {
        return (
          <Tooltip title={`${record}`}>
            <div className='w-52 truncate'>{record ? <span>{record}</span> : <span>N/A</span>}</div>
          </Tooltip>
        );
      },
    },
    {
      title: 'Kết nối',
      width: 100,
      dataIndex: 'matKetNoi',
      render: (val, record) => {
        return (
          <span>{!record.matKetNoiQuanlity ? '????' : val ? 'Mất kết nối' : 'Bình thường'}</span>
        );
      },
    },
    {
      title: 'U',
      align: 'center',
      width: 100,
      dataIndex: 'dienApPhaA',
      render: (val, record) => {
        return <span>{renderValue(val, record.dienApPhaAQuanlity, record.dienApPhaAUnit)}</span>;
      },
    },
    {
      title: 'I',
      align: 'center',
      width: 100,
      dataIndex: 'dongDienPhaA',
      render: (val, record) => {
        return (
          <span>{renderValue(val, record.dongDienPhaAQuanlity, record.dongDienPhaAUnit)}</span>
        );
      },
    },
    {
      title: 'P',
      align: 'center',
      width: 100,
      dataIndex: 'congSuatTacDungTong3Pha',
      render: (val, record) => {
        return (
          <span>
            {renderValue(
              val,
              record.congSuatTacDungTong3PhaQuanlity,
              record.congSuatTacDungTong3PhaUnit
            )}
          </span>
        );
      },
    },
    {
      title: 'Q',
      align: 'center',
      width: 100,
      dataIndex: 'congSuatPhanKhangTong3Pha',
      render: (val, record) => {
        return (
          <span>
            {renderValue(
              val,
              record.congSuatPhanKhangTong3PhaQuanlity,
              record.congSuatPhanKhangTong3PhaUnit
            )}
          </span>
        );
      },
    },
    {
      title: 'PF',
      align: 'center',
      width: 100,
      dataIndex: 'heSoCongSuatPFTrungBinh3Pha',
      render: (val, record) => {
        return <span>{renderValue(val, record.heSoCongSuatPFTrungBinh3PhaQuanlity)}</span>;
      },
    },
    {
      title: 'F',
      align: 'center',
      width: 100,
      dataIndex: 'tanSoHeThong',
      render: (val, record) => {
        return <span>{renderValue(val, record.tanSoHeThong, record.tanSoHeThongUnit)} </span>;
      },
    },
    {
      key: 'action',
      fixed: 'right',
      align: 'left',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              const url = ROUTE_PATH.POINT_MONITORING_DETAIL.replace(
                Constants.ReplaceRouterConfig.id,
                record.measurementPointCode
              );
              window.open(url, '_blank');
            },
          },
        ];

        return (
          <div className={'flex justify-center cursor-pointer'}>
            <Dropdown menu={{ items }}>
              <BsThreeDots />
            </Dropdown>
          </div>
        );
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      rowClassName={(record) => {
        return record.matKetNoi === true ? 'error-row cursor-pointer' : 'cursor-pointer';
      }}
      loading={loading}
      scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
    />
  );
};

export default TableSummary;
