import { useState } from "react";
import CustomButton from "../components/CustomButton";
import CustomInput from "../components/CustomInput";
import CustomStringList from "../components/CustomStringList";

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
    <div
      style={{
        minHeight: "100vh",
        backgroundColor: "#242424",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        padding: "2rem",
      }}
    >
      <div
        style={{
          display: "flex",
          flexDirection: "row",
          gap: "2rem",
          width: "100%",
          maxWidth: "1000px",
          alignItems: "stretch",
          flexWrap: "wrap",
        }}
      >
        <div style={{ flex: 1, minWidth: "300px" }}>
          <CustomStringList title="Feature Highlights" items={items} />
        </div>

        <div
          style={{
            flex: 1,
            minWidth: "250px",
            display: "flex",
            flexDirection: "column",
            justifyContent: "flex-end",
            alignItems: "center",
            gap: "1rem",
            paddingBottom: "100px",
          }}
        >
          <CustomInput
            placeholder="Type something..."
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            style={{ width: "100%" }}
          />
          <CustomButton text="Submit" onClick={handleClick} />
        </div>
      </div>
    </div>
  );
}
