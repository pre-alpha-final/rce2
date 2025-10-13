import { useState } from "react";
import CustomButton from "../components/CustomButton";
import CustomInput from "../components/CustomInput";
import CustomStringList from "../components/CustomStringList";
import "./Rce2.css";
import useRce2 from "./useRce2";

export default function Rce2() {
  const [items, setItems] = useState([
    "dafs asfd asdfdas fasd fasdf asdf asf sadf asg",
    "Banana",
    "Cherry",
    "Dragonfruit",
  ]);
  const [outputValue, setOutputValue] = useState("");
  const [rce2Input$, rce2Send] = useRce2();

  rce2Input$.subscribe(e => {
    setItems([...items, (e.payload as { data: string }).data]);
  });

  const handleClick = async () => {
    await rce2Send('output', {
      data: outputValue
    });
  };

  return (
    <div className="container">
      <div className="content">
        <div className="list">
          <CustomStringList title="Feature Highlights" items={items} />
        </div>

        <div className="input-section">
          <CustomInput
            placeholder="Type something..."
            value={outputValue}
            onChange={(e) => setOutputValue(e.target.value)}
            className="custom-input width-100"
          />
          <CustomButton text="Submit" onClick={handleClick} />
        </div>
      </div>
    </div>
  );
}
