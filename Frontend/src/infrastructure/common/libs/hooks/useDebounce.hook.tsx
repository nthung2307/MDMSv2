import { useEffect, useState, useRef } from 'react';

function useDebounce<T>(value: T, delay: number = 500): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);
  const previousValue = useRef(value);

  useEffect(() => {
    if (previousValue.current === value) return; // Skip update if the value hasn't changed

    const timer = setTimeout(() => {
      setDebouncedValue(value);
      previousValue.current = value; // Update previous value reference
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default useDebounce;
