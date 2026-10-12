import { Button } from 'antd';
import { useEffect } from 'react';

import { useLogoutHook } from '@/infrastructure/repository/auth/hooks/useLogout.hook';

const Test = () => {
  const [requestLogout] = useLogoutHook();

  function handleTest() {
    requestLogout(
      {
        email: '',
        password: '',
      },
      () => {
        // handleTest()
      },
      () => {
        // handleTest()
      }
    );
  }
  useEffect(() => {
    handleTest();
  }, []);
  return (
    <div>
      <Button
        onClick={() => {
          handleTest();
        }}
      >
        Login
      </Button>
      <h1>Login Page</h1>
    </div>
  );
};

export default Test;
