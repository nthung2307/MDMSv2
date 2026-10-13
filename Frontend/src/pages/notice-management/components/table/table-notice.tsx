import { TableProps } from 'antd';

import { CheckboxCommon } from '@/infrastructure/common/components/Checkbox/checkbox-common';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import { convertDateTime } from '@/infrastructure/utils/helpers.ts';

interface Props {
  dataSource: any;
  sendData: any;
  setSendData: any;
  setIsShowDialogPDF: any;
  loading: boolean;
}

const TableDetailNotice = (props: Props) => {
  const { dataSource, setSendData, sendData, setIsShowDialogPDF, loading } = props;
  const addSendMail = (record: any) => {
    let arr = [...sendData.listSend];
    const index = arr.findIndex((item) => item.id === record.id);
    if (index !== -1) {
      arr = arr.filter((item) => item.id !== record.id);
    } else {
      arr = [...arr, record];
    }
    setSendData({ isAll: false, listSend: arr });
  };

  const columns: TableProps['columns'] = [
    {
      title: (
        <div>
          <CheckboxCommon
            dataAttribute={sendData.isAll}
            setData={() => {
              setSendData({
                listSend: sendData.isAll ? [] : [...dataSource],
                isAll: !sendData.isAll,
              });
            }}
            disabled={!(dataSource.length > 0)}
            label={undefined}
          />
        </div>
      ),
      dataIndex: 'measurementPoint',
      width: '5%',
      render: (_, record) => {
        return (
          <div>
            <CheckboxCommon
              dataAttribute={
                !!sendData.listSend.find((item: any) => item.id === record.id) || sendData.isAll
              }
              setData={() => {
                addSendMail(record);
              }}
              label={undefined}
            />
          </div>
        );
      },
    },
    {
      title: 'Mã khách hàng',
      dataIndex: 'measurementPoint',
      render: (value) => {
        return <div>{value.customer.customerCode}</div>;
      },
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'measurementPoint',
      render: (record) => {
        return <div>{record.customer.fullName}</div>;
      },
    },
    {
      title: 'Mã điểm đo',
      dataIndex: 'measurementPoint',
      render: (record) => {
        return <div>{record.code}</div>;
      },
    },
    {
      title: 'Trạng thái gửi Email',
      dataIndex: 'status',
      render: (record) => {
        return <div>{record == 'SEND_SUCCESS' ? 'Thành công' : ''}</div>;
      },
    },
    {
      title: 'Thời gian gửi gần nhất',
      dataIndex: 'sentTime',
      render: (record) => {
        return <div>{record ? convertDateTime(record, 'dd/mm/yyyy hh:mi:ss') : ''}</div>;
      },
    },
  ];

  return (
    <TableCommon
      columns={columns}
      dataSource={dataSource}
      pagination={false}
      // scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
      onRow={(record) => {
        return {
          onClick: (e) => {
            if ((e.target as HTMLInputElement).type !== 'checkbox') {
              setIsShowDialogPDF({ status: true, record });
            }
          },
        };
      }}
      loading={loading}
    />
  );
};

export default TableDetailNotice;
