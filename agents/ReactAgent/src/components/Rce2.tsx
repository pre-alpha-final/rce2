import { useEffect, useState } from "react";
import CustomButton from "./CustomButton";
import CustomInput from "./CustomInput";
import CustomStringList from "./CustomStringList";
import "./Rce2.css";
import useRce2 from "../rce2/useRce2";

export default function Rce2() {
  const [inputItems, setInputItems] = useState<string[]>([]);
  const [outputValue, setOutputValue] = useState("");
  const [rce2Input$, rce2Send] = useRce2();

  useEffect(() => {
    const subscription = rce2Input$.subscribe((e) => {
      setInputItems((f) => [...f, (e.payload as { data: string }).data]);
    });

    return () => {
      subscription.unsubscribe();
    };
  }, [rce2Input$]);

  const outputOnClick = async () => {
    await rce2Send("output", {
      data: outputValue,
    });
  };

  return (
    <div className="container">
      <div className="content">
        <div className="list">
          <CustomStringList title="Inputs" items={inputItems} />
        </div>

        <div className="output-section">
          <CustomInput
            placeholder="Type something..."
            value={outputValue}
            onChange={(e) => setOutputValue(e.target.value)}
            className="custom-input width-100"
          />
          <CustomButton text="Send" onClick={outputOnClick} />
        </div>
      </div>
    </div>
  );
}
