import { Dispatch, SetStateAction, useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants.ts';
import { ChangePasswordRequest } from '@/core/application/dto/identity/auth/requests/ChangePassword.ts';
import InputPasswordCommon from '@/infrastructure/common/components/input/input-password-common.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useChangePasswordHook from '@/infrastructure/repository/auth/hooks/useChangePassword.hook.tsx';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

interface IChangePasswordModalProps {
  title?: string;
  visible: boolean;
  setVisible: Dispatch<SetStateAction<boolean>>;
  setLoading: Dispatch<SetStateAction<boolean>>;
  buttonLoading?: boolean;
}

const ChangePasswordModal = ({
  title = '',
  visible,
  setVisible,
  setLoading,
  buttonLoading,
}: IChangePasswordModalProps) => {
  const [updatePasswordRequest] = useChangePasswordHook();
  const [validate, setValidate] = useState<any>({});
  const [isDisabledSubmitButton, setIsDisabledSubmitButton] = useState<boolean>(true);
  const [dataForm, setDataForm] = useStateDataForm({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>();

  const updatePasswordAsync = async () => {
    setSubmittedTime(new Date());
    if (!checkIsValid()) return;
    setLoading(true);
    const params = new ChangePasswordRequest({
      ...dataForm,
    });
    await updatePasswordRequest(
      params,
      () => {
        notifySuccess('', _t('Đổi mật khẩu thành công'));
        setVisible(!visible);
      },
      () => {},
      () => {}
    );
    setSubmittedTime(null);
    setLoading(false);
  };

  const checkIsValid = () => {
    return (
      dataForm?.newPassword === dataForm?.confirmPassword &&
      isValidData(validate) &&
      !isFormFieldsEmpty(['oldPassword', 'newPassword', 'confirmPassword'], dataForm)
    );
  };

  useEffect(() => {
    if (dataForm?.newPassword === dataForm?.confirmPassword) {
      setValidate({
        ...validate,
        confirmPassword: {
          isError: false,
          message: '',
        },
      });
      setIsDisabledSubmitButton(false);
    } else {
      setValidate({
        ...validate,
        confirmPassword: {
          isError: true,
          message: _t('Mật khẩu nhập lại không khớp'),
        },
      });
      setIsDisabledSubmitButton(true);
    }
  }, [dataForm?.confirmPassword, dataForm?.newPassword]);

  useEffect(() => {
    setIsDisabledSubmitButton(!checkIsValid());
  }, []);

  return (
    <>
      <DialogCommon
        title={title}
        visible={visible}
        handleCancel={() => {
          setDataForm({
            oldPassword: '',
            newPassword: '',
            confirmPassword: '',
          });
          setValidate({
            oldPassword: {
              isError: false,
              message: '',
            },
            newPassword: {
              isError: false,
              message: '',
            },
            confirmPassword: {
              isError: false,
              message: '',
            },
          });
          setVisible(!visible);
        }}
        handleConfirm={updatePasswordAsync}
        isButtonSubmit={true}
        showFooter={true}
        disableSubmitButton={isDisabledSubmitButton}
        buttonLoading={buttonLoading}
      >
        <div className='flex flex-col gap-3 my-5'>
          <div>
            <InputPasswordCommon
              dataAttribute={dataForm.oldPassword}
              autoFocus={true}
              attribute={'oldPassword'}
              isRequired={true}
              label={_t('Mật khẩu cũ')}
              setData={setDataForm}
              placeholder={_t('Nhập mật khẩu cũ')}
              setValidate={setValidate}
              validate={validate}
              submittedTime={submittedTime}
              // validateOnBlur={false}
            />
          </div>
          <div>
            <InputPasswordCommon
              dataAttribute={dataForm.newPassword}
              autoFocus={true}
              attribute={'newPassword'}
              isRequired={true}
              label={_t('Mật khẩu mới')}
              setData={setDataForm}
              placeholder={_t('Nhập mật khẩu mới')}
              setValidate={setValidate}
              validate={validate}
              rules={[
                {
                  pattern: Constants.PATTERN_REGEX.PasswordStrong,
                  message: _t(
                    'Mật khẩu phải chứa ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt'
                  ),
                },
              ]}
              submittedTime={submittedTime}
              // validateOnBlur={false}
            />
          </div>
          <div>
            <InputPasswordCommon
              dataAttribute={dataForm.confirmPassword}
              attribute={'confirmPassword'}
              isRequired={true}
              label={_t('Nhập lại mật khẩu mới')}
              setData={setDataForm}
              placeholder={_t('Nhập lại mật khẩu mới')}
              setValidate={setValidate}
              validate={validate}
              submittedTime={submittedTime}
              // validateOnBlur={false}
            />
          </div>
        </div>
      </DialogCommon>
    </>
  );
};

export default ChangePasswordModal;
