import { Col, Modal, Row } from 'antd';
import { useEffect, useState } from 'react';

// import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListMeasurementPointsRequest } from '@/core/application/dto/measurement-point/requests/GetListMeasurementPointsRequest';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListMeasurementPointsHook from '@/infrastructure/repository/measurement-point/hooks/useGetListMeasurementPoints.hook';
import { _t } from '@/infrastructure/utils/translation.ts';

type Props = {
  open: boolean;
  handleCancel: () => void;
  handelSubmit: any;
  setGCSName: any;
  loading?: boolean;
};

const DialogDuplicate = (props: Props) => {
  const { handleCancel, open, handelSubmit, loading } = props;
  const [getListMeasurementPoints] = useGetListMeasurementPointsHook();

  const [data, setData, resetData] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<any>(null);
  const [listGcs, setListGcs] = useState<any>([]);

  const resetForm = () => {
    resetData();
    setValidate({});
    handleCancel();
    setSubmittedTime(Date());
  };

  const handleDuplicateRequest = async () => {
    await setSubmittedTime(new Date());
    const arr = data.gcsId;
    if (arr && arr[0]) {
      resetData();
      handelSubmit(arr).then(() => {
        handleCancel();
      });
    }
    setSubmittedTime(null);
  };

  const getListMeasurementPointsAsync = async () => {
    const params: GetListMeasurementPointsRequest = {
      pageSize: 100,
      searchByFields: [],
      sortByFields: [],
    };

    await getListMeasurementPoints(
      params,
      (res: DataResponse) => {
        const { data } = res;
        setListGcs(
          data.data.map((item: any) => {
            return { value: item.id, label: item.code };
          })
        );
      },
      () => {}
    );
  };

  useEffect(() => {
    getListMeasurementPointsAsync();
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
          label={_t('Chọn điểm đo')}
          attribute={'gcsId'}
          dataAttribute={data['gcsId']}
          setData={setData}
          placeholder={_t('Chọn điểm đo')}
          isRequired={true}
          mode={'tags'}
          validate={validate}
          setValidate={setValidate}
          submittedTime={submittedTime}
          disabled={false}
          listDataItem={listGcs}
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
              onClick={handleDuplicateRequest}
              classButton={'w-full'}
              loading={loading}
            />
          </Col>
        </Row>
      </div>
    </Modal>
  );
};
export default DialogDuplicate;
