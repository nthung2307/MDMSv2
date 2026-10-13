import { MinusOutlined, PlusOutlined } from '@ant-design/icons';
import { Col, Row, Tooltip } from 'antd';

import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import { _t } from '@/infrastructure/utils/translation.ts';

interface MultiLevelInputProps {
  setDataApprover?: any;
  validate?: any;
  setValidate?: any;
  submittedTime?: any;
  listUsersEmployee: Array<any>;
  isEdit?: boolean;
  dataMultiLevel: Array<any>;
  setDataMultiLevel: any;
}

const MultiLevelInput = (props: MultiLevelInputProps) => {
  const {
    // listUsersEmployee,
    // setDataApprover,
    validate,
    setValidate,
    submittedTime,
    isEdit = false,
    dataMultiLevel,
    setDataMultiLevel,
  } = props;

  const onAddLevel = () => {
    const newData = [
      ...dataMultiLevel,
      {
        warning: '',
      },
    ];
    setDataMultiLevel(newData);
  };

  const removeLevel = (index: number) => {
    if (dataMultiLevel.length <= 1) {
      return;
    }
    const newData = [...dataMultiLevel];
    newData.splice(index, 1);
    setDataMultiLevel(newData);
  };

  return (
    <div className='multiple-input flex flex-col gap-1'>
      <Row className='flex justify-between'>
        <Col className={'font-bold text-14-20 text-[#3B3B3B]'}>
          <label>{_t('Danh sách cảnh báo')}</label>
        </Col>
        <Col>
          {isEdit && (
            <Tooltip title={_t('Thêm cảnh báo')}>
              <div
                className='cursor-pointer text-red-F51616 mr-3'
                onClick={() => {
                  onAddLevel();
                }}
              >
                <PlusOutlined />
              </div>
            </Tooltip>
          )}
        </Col>
      </Row>
      {dataMultiLevel.length &&
        dataMultiLevel.map((_item, index) => {
          return (
            <Row key={index} align={'top'}>
              <Col span={24}>
                <InputTextCommon
                  label={_t(``)}
                  isLabel={false}
                  attribute={'warning'}
                  setData={setDataMultiLevel}
                  isRequired={false}
                  dataAttribute={_item.warning}
                  placeholder={_t(`Điền tên cảnh báo`)}
                  disabled={!isEdit}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  suffix={
                    isEdit && (
                      <div
                        className={`${dataMultiLevel.length <= 1 ? 'cursor-not-allowed text-gray-516078' : 'cursor-pointer text-red-F51616'}`}
                        onClick={() => {
                          removeLevel(index);
                        }}
                      >
                        <MinusOutlined />
                      </div>
                    )
                  }
                  onCallbackFunction={(data: any) => {
                    setDataMultiLevel((prev: any) => {
                      let _data = [...prev];
                      _data[index].warning = data;
                      return _data;
                    });
                  }}
                />
              </Col>
            </Row>
          );
        })}
    </div>
  );
};

export default MultiLevelInput;
