import { useEffect, useState } from 'react';

interface KeyPressInfo {
  key: string;
  pressed: boolean;
}

const useKeyPressHook = (targetKeys: string | string[], returnDetailedInfo = false) => {
  // State to track whether the key(s) are pressed
  const [keyPressed, setKeyPressed] = useState<boolean>(false);

  // State to track detailed key press information
  const [keyPressInfo, setKeyPressInfo] = useState<KeyPressInfo[]>(
    Array.isArray(targetKeys)
      ? targetKeys.map((key) => ({ key, pressed: false }))
      : [{ key: targetKeys, pressed: false }]
  );

  const targetKeyArray = Array.isArray(targetKeys) ? targetKeys : [targetKeys];

  // Function to handle key down events
  const downHandler = (event: KeyboardEvent) => {
    if (targetKeyArray.includes(event.key)) {
      setKeyPressed(true);

      // Update the pressed state of each key in detailed mode
      if (returnDetailedInfo) {
        setKeyPressInfo((prev) =>
          prev.map((item) => (item.key === event.key ? { ...item, pressed: true } : item))
        );
      }
    }
  };

  // Function to handle key up events
  const upHandler = (event: KeyboardEvent) => {
    if (targetKeyArray.includes(event.key)) {
      setKeyPressed(false);

      // Update the pressed state of each key in detailed mode
      if (returnDetailedInfo) {
        setKeyPressInfo((prev) =>
          prev.map((item) => (item.key === event.key ? { ...item, pressed: false } : item))
        );
      }
    }
  };

  useEffect(() => {
    // Add event listeners for keydown and keyup
    window.addEventListener('keydown', downHandler);
    window.addEventListener('keyup', upHandler);

    // Cleanup event listeners on component unmount
    return () => {
      window.removeEventListener('keydown', downHandler);
      window.removeEventListener('keyup', upHandler);
    };
  }, [targetKeyArray]); // Re-run if the targetKeys change

  // Return detailed information if requested, else just the boolean state
  return returnDetailedInfo ? keyPressInfo : keyPressed;
};

export default useKeyPressHook;
