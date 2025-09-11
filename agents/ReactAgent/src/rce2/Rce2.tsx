import { useState } from "react";
import CustomButton from "../components/CustomButton";
import CustomInput from "../components/CustomInput";
import CustomStringList from "../components/CustomStringList";
import "./Rce2.css"; // import the stylesheet

export default function Rce2() {
  const items = [
    "dafs asfd asdfdas fasd fasdf asdf asf sadf asg",
    "Banana",
    "Cherry",
    "Dragonfruit",
  ];

  const [inputValue, setInputValue] = useState("");

  const handleClick = () => {
    alert(`You entered: ${inputValue}`);
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
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            className="custom-input width-100"
          />
          <CustomButton text="Submit" onClick={handleClick} />
        </div>
      </div>
    </div>
  );
}
