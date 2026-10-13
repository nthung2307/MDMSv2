import { Col, Modal, Row } from 'antd';
import { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListGCSHook from '@/infrastructure/repository/gcs/hooks/useGetGCSList.hook';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  open: boolean;
  handleCancel: () => void;
  handelSubmit: any;
  setGCSName: any;
};

const DialogNotice = (props: Props) => {
  const { handleCancel, open, handelSubmit, setGCSName } = props;

  const [data, setData, resetData] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<any>(null);
  const [listGcs, setListGcs] = useState<any>([]);
  const [getListGCS] = useGetListGCSHook();
  const isValidData = () => {
    let allRequestOK = true;
    setValidate({ ...validate });
    Object.values(validate).forEach((it: any) => {
      if (it.isError === true) {
        allRequestOK = false;
      }
    });

    return allRequestOK;
  };

  const getListGCSAsync = async () => {
    const params = {
      pageSize: 100,
      searchByFields: [
        {
          searchFieldName: 'status',
          searchValue: Constants.CustomerStatus.Active,
        },
      ],
    };
    await getListGCS(
      params,
      (res: DataResponse) => {
        const { data } = res;
        setListGcs(data.data);
      },
      () => {},
      () => {}
    );
  };
  const resetForm = () => {
    resetData();
    setValidate({});
    handleCancel();
    setSubmittedTime(Date());
  };

  const handleGetListGcs = async () => {
    await setSubmittedTime(new Date());
    if (isValidData()) {
      const nameGcs = listGcs.find((gcs: any) => gcs.id == data.gcsId);
      resetData();
      handleCancel();
      setGCSName(nameGcs.name);
      handelSubmit({
        gcsId: data.gcsId,
      }).then(() => {
        setSubmittedTime(null);
      });
    }
  };

  useEffect(() => {
    getListGCSAsync();
  }, []);

  return (
    <Modal
      open={open}
      key={'f-0'}
      centered
      className='min-w-[650px]'
      closable={true}
      footer={false}
      onCancel={resetForm}
    >
      <div className='modal space-y-4'>
        <div className={`title font-extrabold text-16-24 text-center`}>{_t('Vui lòng chọn')}</div>

        <InputSelectCommon
          label={_t('Chọn sổ chốt')}
          attribute={'gcsId'}
          dataAttribute={data['gcsId']}
          setData={setData}
          placeholder={_t('Chọn sổ chốt')}
          isRequired={true}
          validate={validate}
          setValidate={setValidate}
          submittedTime={submittedTime}
          disabled={false}
          listDataItem={listGcs.map((item: any) => {
            return {
              label: item.name,
              value: item.id,
            };
          })}
        />
        <Row gutter={8} className='w-3/4 m-auto'>
          <Col span={12}>
            <ButtonCommon
              title={_t('Hủy')}
              colorClass={'firebrick-outline-bg'}
              onClick={resetForm}
              classButton={'w-full'}
            />
          </Col>
          <Col span={12}>
            <ButtonCommon
              title={_t('Xác nhận')}
              colorClass={'firebrick-bg'}
              onClick={handleGetListGcs}
              classButton={'w-full'}
            />
          </Col>
        </Row>
      </div>
    </Modal>
  );
};
export default DialogNotice;
