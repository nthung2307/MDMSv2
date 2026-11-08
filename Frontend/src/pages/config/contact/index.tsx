import React, { useEffect, useState } from 'react';
import { Card, Col, Divider, Row, Typography } from 'antd';
import { TbBook2 } from 'react-icons/tb';
import { BankOutlined, CustomerServiceOutlined, UserOutlined } from '@ant-design/icons';

import Constants from '@/core/application/common/constants';
import { systemConfigRequest } from '@/core/application/dto/tag-name/requests/SystemConfigRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useCreateUpdateSystemConfigHook from '@/infrastructure/repository/config/hooks/useCreateUpdateSystemConfig.hook';
import useGetSystemListHook from '@/infrastructure/repository/config/hooks/useGetSystemList.hook';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import { getCachedStatementTemplate, setCachedStatementTemplate } from '@/pages/notice-management/components/form-pdf';

const { Text } = Typography;

const ContactConfig = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const [createUpdateSystemConfig] = useCreateUpdateSystemConfigHook();
  const [getSystemList] = useGetSystemListHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);

  const isDisabled =
    !isValidData(validate) ||
    isFormFieldsEmpty(
      [
        'companyName',
        'companyTaxCode',
        'companyPhone',
        'adminName',
        'adminNumber',
        'adminEmail',
        'supporterName',
        'supporterNumber',
        'supporterEmail',
      ],
      dataForm
    );

  const getSystemListAsync = async () => {
    await getSystemList(
      (res: any) => {
        const { data } = res;
        const findVal = (key: string, defVal: string = '') =>
          data?.find((x: any) => x.key === key)?.value ?? defVal;

        const rawDetail = findVal(Constants.LockAuth.COMPANY_ADDRESS_DETAIL);
        const rawFull = findVal(Constants.LockAuth.COMPANY_ADDRESS);
        let defaultDetail = rawDetail;
        if (!defaultDetail && rawFull) {
          defaultDetail = rawFull.includes('\n') ? rawFull.split('\n')[0]?.trim() : rawFull;
        }

        const resultData = {
          // Thông tin Đơn vị cung cấp điện tập trung
          companyName: findVal(Constants.LockAuth.COMPANY_NAME, 'CÔNG TY TNHH VSIP HẢI PHÒNG'),
          companyTaxCode: findVal(Constants.LockAuth.COMPANY_TAX_CODE, '0200856991'),
          companyPhone: findVal(Constants.LockAuth.COMPANY_PHONE, '84-225-3959868'),
          companyFax: findVal(Constants.LockAuth.COMPANY_FAX, '84-225-3959886'),
          companyEmail: findVal(Constants.LockAuth.COMPANY_EMAIL, 'CustomerCare@vsip.com.vn'),
          companyWebsite: findVal(Constants.LockAuth.COMPANY_WEBSITE, 'https://vsip.com.vn'),
          companyProvince: findVal(Constants.LockAuth.COMPANY_PROVINCE, 'Hai Phong City'),
          companyDistrict: findVal(Constants.LockAuth.COMPANY_DISTRICT, ''),
          companyWard: findVal(Constants.LockAuth.COMPANY_WARD, 'Hoa Binh Ward'),
          companyAddressDetail:
            defaultDetail ||
            'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park',
          companyCountry: findVal(Constants.LockAuth.COMPANY_COUNTRY, 'Vietnam'),
          companyAddress:
            rawFull ||
            'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',

          // Quản trị hệ thống
          adminName: findVal(Constants.LockAuth.SYSTEMADMINISTRATIONFULLNAME, 'Administrator'),
          adminNumber: findVal(Constants.LockAuth.SYSTEMADMINISTRATIONPHONE, '0901234567'),
          adminEmail: findVal(Constants.LockAuth.SYSTEMADMINISTRATIONEMAIL, 'admin@mdms.vn'),

          // Chăm sóc khách hàng
          supporterName: findVal(Constants.LockAuth.CUSTOMERCAREFULLNAME, 'CSKH'),
          supporterNumber: findVal(Constants.LockAuth.CUSTOMERCAREPHONE, '19001006'),
          supporterEmail: findVal(Constants.LockAuth.CUSTOMERCAREEMAIL, 'cskh@mdms.vn'),
        };
        setDataForm(resultData);
      },
      () => {},
      setLoading
    );
  };

  const executeSaveConfigsAsync = async () => {
    // Tự động cấu trúc Địa chỉ 2 dòng theo chuẩn: Dòng 1 {Địa chỉ} & Dòng 2 {Phường/Xã}, {Tỉnh/TP}, Vietnam
    const line1 = (dataForm.companyAddressDetail || '').trim();
    const ward = (dataForm.companyWard || '').trim();
    const province = (dataForm.companyProvince || '').trim();
    const country = (dataForm.companyCountry || 'Vietnam').trim();
    const line2Parts = [ward, province, country].filter(Boolean);
    const line2 = line2Parts.join(', ');
    const computedCompanyAddress = [line1, line2].filter(Boolean).join('\n');

    const params = new systemConfigRequest({
      configs: [
        // 1. Thông tin Đơn vị cung cấp điện tập trung (dùng chung cho Mẫu phiếu báo & Mẫu email)
        {
          key: Constants.LockAuth.COMPANY_NAME,
          value: dataForm.companyName || '',
        },
        {
          key: Constants.LockAuth.COMPANY_TAX_CODE,
          value: dataForm.companyTaxCode || '',
        },
        {
          key: Constants.LockAuth.COMPANY_PHONE,
          value: dataForm.companyPhone || '',
        },
        {
          key: Constants.LockAuth.COMPANY_FAX,
          value: dataForm.companyFax || '',
        },
        {
          key: Constants.LockAuth.COMPANY_EMAIL,
          value: dataForm.companyEmail || '',
        },
        {
          key: Constants.LockAuth.COMPANY_WEBSITE,
          value: dataForm.companyWebsite || '',
        },
        {
          key: Constants.LockAuth.COMPANY_PROVINCE,
          value: dataForm.companyProvince || '',
        },
        {
          key: Constants.LockAuth.COMPANY_DISTRICT,
          value: dataForm.companyDistrict || '',
        },
        {
          key: Constants.LockAuth.COMPANY_WARD,
          value: dataForm.companyWard || '',
        },
        {
          key: Constants.LockAuth.COMPANY_ADDRESS_DETAIL,
          value: dataForm.companyAddressDetail || '',
        },
        {
          key: Constants.LockAuth.COMPANY_COUNTRY,
          value: country,
        },
        {
          key: Constants.LockAuth.COMPANY_ADDRESS,
          value: computedCompanyAddress,
        },

        // 2. Quản trị hệ thống
        {
          key: Constants.LockAuth.SYSTEMADMINISTRATIONFULLNAME,
          value: dataForm.adminName || '',
        },
        {
          key: Constants.LockAuth.SYSTEMADMINISTRATIONPHONE,
          value: dataForm.adminNumber || '',
        },
        {
          key: Constants.LockAuth.SYSTEMADMINISTRATIONEMAIL,
          value: dataForm.adminEmail || '',
        },

        // 3. Chăm sóc khách hàng
        {
          key: Constants.LockAuth.CUSTOMERCAREFULLNAME,
          value: dataForm.supporterName || '',
        },
        {
          key: Constants.LockAuth.CUSTOMERCAREPHONE,
          value: dataForm.supporterNumber || '',
        },
        {
          key: Constants.LockAuth.CUSTOMERCAREEMAIL,
          value: dataForm.supporterEmail || '',
        },
      ],
    });

    setSubmittedTime(new Date());
    setIsShowDialog(false);

    await createUpdateSystemConfig(
      params,
      () => {
        // Cập nhật bộ đệm Mẫu phiếu báo để toàn hệ thống đồng bộ tức thời
        const activeStmt = getCachedStatementTemplate();
        if (activeStmt) {
          setCachedStatementTemplate({
            ...activeStmt,
            companyName: dataForm.companyName,
            companyAddress: computedCompanyAddress,
            contactPhone: dataForm.companyPhone,
          });
        }
        notifySuccess('', 'Đã lưu và đồng bộ thông tin đơn vị cho toàn hệ thống (Phiếu báo & Email)');
      },
      () => {},
      setLoading
    );
    setSubmittedTime(null);
  };

  useEffect(() => {
    void getSystemListAsync();
  }, []);

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Thông tin liên hệ' }]} />
        <div className='flex'>
          <ButtonCommonCustomizable
            title={''}
            tooltipText={'Lưu thông tin'}
            classButton='text-button-variant'
            disabled={isDisabled}
            onClick={() => {
              setIsShowDialog(true);
            }}
            icon={<TbBook2 size={24} />}
          />
        </div>
      </div>

      <div className='mt-5 space-y-6'>
        {/* KHỐI 1: THÔNG TIN CÔNG TY / ĐƠN VỊ CUNG CẤP ĐIỆN */}
        <Card
          className='shadow-custom'
          title={
            <div className='flex items-center gap-2'>
              <BankOutlined style={{ color: '#003366', fontSize: 18 }} />
              <span className='font-bold text-16-20 text-[#003366]'>
                Thông tin Công ty/Đơn vị cung cấp điện
              </span>
            </div>
          }
        >
          <div className='space-y-4'>
            <Row gutter={16}>
              <Col xs={24} md={16}>
                <InputTextCommon
                  label={_t('Tên công ty / Đơn vị cung cấp điện')}
                  attribute={'companyName'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyName}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={'Ví dụ: CÔNG TY TNHH VSIP HẢI PHÒNG'}
                />
              </Col>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Mã số thuế (MST)')}
                  attribute={'companyTaxCode'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyTaxCode}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={'Ví dụ: 0200856991'}
                />
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Điện thoại')}
                  attribute={'companyPhone'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyPhone}
                  isRequired={true}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  rules={[
                    {
                      message: _t('Số điện thoại không đúng định dạng (Ví dụ: 84-225-3959868, 0225 3959868)'),
                      pattern: Constants.PATTERN_REGEX.PhoneNumber,
                    },
                  ]}
                  placeholder={'Ví dụ: 84-225-3959868 hoặc 0225 3959868'}
                />
              </Col>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Fax')}
                  attribute={'companyFax'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyFax}
                  isRequired={false}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  rules={[
                    {
                      message: _t('Số Fax không đúng định dạng'),
                      pattern: Constants.PATTERN_REGEX.PhoneNumber,
                    },
                  ]}
                  placeholder={'Ví dụ: 84-225-3959886'}
                />
              </Col>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Email')}
                  attribute={'companyEmail'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyEmail}
                  isRequired={false}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={'Ví dụ: CustomerCare@vsip.com.vn'}
                />
              </Col>
            </Row>

            <Divider orientation='left' style={{ margin: '16px 0 8px 0', fontSize: 13, color: '#64748b' }}>
              Địa chỉ đơn vị (Tự động hiển thị 2 dòng trên Phiếu báo: Dòng 1 [Địa chỉ] & Dòng 2 [Phường/Xã, Tỉnh/TP, Quốc gia])
            </Divider>

            <Row gutter={16}>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Quốc gia')}
                  attribute={'companyCountry'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyCountry}
                  isRequired={false}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={'Ví dụ: Vietnam hoặc Việt Nam'}
                />
              </Col>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Tỉnh / Thành phố')}
                  attribute={'companyProvince'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyProvince}
                  isRequired={false}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={'Ví dụ: Hai Phong City hoặc TP. Hải Phòng'}
                />
              </Col>
              <Col xs={24} md={8}>
                <InputTextCommon
                  label={_t('Phường/Xã')}
                  attribute={'companyWard'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyWard}
                  isRequired={false}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={'Ví dụ: Hoa Binh Ward hoặc Xã An Lư'}
                />
              </Col>
            </Row>

            <Row gutter={16}>
              <Col xs={24} md={24}>
                <InputTextCommon
                  label={_t('Địa chỉ')}
                  attribute={'companyAddressDetail'}
                  setData={setDataForm}
                  dataAttribute={dataForm?.companyAddressDetail}
                  isRequired={false}
                  validate={validate}
                  setValidate={setValidate}
                  submittedTime={submittedTime}
                  placeholder={
                    'Ví dụ: VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park'
                  }
                />
              </Col>
            </Row>
          </div>
        </Card>

        {/* KHỐI 2: QUẢN TRỊ HỆ THỐNG */}
        <Card
          className='shadow-custom'
          title={
            <div className='flex items-center gap-2'>
              <UserOutlined style={{ color: '#003366', fontSize: 18 }} />
              <span className='font-bold text-16-20 text-[#003366]'>Quản trị hệ thống</span>
            </div>
          }
        >
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <InputTextCommon
                label={_t('Họ và tên')}
                attribute={'adminName'}
                setData={setDataForm}
                dataAttribute={dataForm?.adminName}
                isRequired={true}
                validate={validate}
                setValidate={setValidate}
                submittedTime={submittedTime}
                placeholder={'Nhập họ và tên quản trị'}
              />
            </Col>
            <Col xs={24} md={8}>
              <InputTextCommon
                label={_t('Điện thoại')}
                attribute={'adminNumber'}
                setData={setDataForm}
                dataAttribute={dataForm?.adminNumber}
                isRequired={true}
                validate={validate}
                setValidate={setValidate}
                submittedTime={submittedTime}
                rules={[
                  {
                    message: _t('Số điện thoại không đúng định dạng (+84/0xxx)'),
                    pattern: Constants.PATTERN_REGEX.PhoneNumber,
                  },
                ]}
                placeholder={'Ví dụ: 0901234567 hoặc 024-3959888'}
              />
            </Col>
            <Col xs={24} md={8}>
              <InputTextCommon
                label={_t('Email')}
                attribute={'adminEmail'}
                setData={setDataForm}
                dataAttribute={dataForm?.adminEmail}
                isRequired={true}
                validate={validate}
                setValidate={setValidate}
                submittedTime={submittedTime}
                rules={[
                  {
                    message: _t('Vui lòng nhập đúng định dạng email'),
                    pattern: Constants.PATTERN_REGEX.Email,
                  },
                ]}
                placeholder={'Nhập Email quản trị'}
              />
            </Col>
          </Row>
        </Card>

        {/* KHỐI 3: CHĂM SÓC KHÁCH HÀNG */}
        <Card
          className='shadow-custom'
          title={
            <div className='flex items-center gap-2'>
              <CustomerServiceOutlined style={{ color: '#003366', fontSize: 18 }} />
              <span className='font-bold text-16-20 text-[#003366]'>
                Chăm sóc khách hàng
              </span>
            </div>
          }
        >
          <Row gutter={16}>
            <Col xs={24} md={8}>
              <InputTextCommon
                label={_t('Bộ phận')}
                attribute={'supporterName'}
                setData={setDataForm}
                dataAttribute={dataForm?.supporterName}
                isRequired={true}
                validate={validate}
                setValidate={setValidate}
                submittedTime={submittedTime}
                placeholder={'Ví dụ: Bộ phận Chăm sóc khách hàng'}
              />
            </Col>
            <Col xs={24} md={8}>
              <InputTextCommon
                label={_t('Hotline')}
                attribute={'supporterNumber'}
                setData={setDataForm}
                dataAttribute={dataForm?.supporterNumber}
                isRequired={true}
                validate={validate}
                setValidate={setValidate}
                submittedTime={submittedTime}
                rules={[
                  {
                    message: _t('Hotline/Số điện thoại không đúng định dạng'),
                    pattern: Constants.PATTERN_REGEX.PhoneNumber,
                  },
                ]}
                placeholder={'Ví dụ: 19001006 hoặc 0904359568'}
              />
            </Col>
            <Col xs={24} md={8}>
              <InputTextCommon
                label={_t('Email')}
                attribute={'supporterEmail'}
                setData={setDataForm}
                dataAttribute={dataForm?.supporterEmail}
                isRequired={true}
                validate={validate}
                setValidate={setValidate}
                submittedTime={submittedTime}
                rules={[
                  {
                    message: _t('Vui lòng nhập đúng định dạng email'),
                    pattern: Constants.PATTERN_REGEX.Email,
                  },
                ]}
                placeholder={'Ví dụ: cskh@mdms.vn'}
              />
            </Col>
          </Row>
        </Card>
      </div>

      <FullPageLoading isLoading={loading} />

      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(false);
        }}
        handleConfirm={() => {
          executeSaveConfigsAsync();
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <p className='text-16-24 font-bold'>{_t('Xác nhận lưu thông tin liên hệ & Đơn vị')}</p>
            <p className='text-center text-gray-600 mt-2'>
              {_t(
                'Sau khi xác nhận, toàn bộ thông tin Đơn vị cung cấp điện, MST, Địa chỉ và Đầu mối CSKH sẽ được cập nhật đồng bộ cho Mẫu phiếu báo và Mẫu email trên toàn hệ thống.'
              )}
            </p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default ContactConfig;
