import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import StringList from './StringList';
import DarkInput from './DarkInput';
import DarkButton from './DarkButton';

function App() {
  const [count, setCount] = useState(0);
  const items = ["dafs asfd asdfdas fasd fasdf asdf asf sadf asg", "Banana", "Cherry", "Dragonfruit"];
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
          alignItems: "stretch", // Stretch columns to same height
          flexWrap: "wrap",
        }}
      >
        {/* Left Column */}
        <div style={{ flex: 1, minWidth: "300px" }}>
          <StringList title="Feature Highlights" items={items} />
        </div>

        {/* Right Column */}
        <div
          style={{
            flex: 1,
            minWidth: "250px",
            display: "flex",
            flexDirection: "column",
            justifyContent: "flex-end", // Stick input/button to bottom
            alignItems: "center",
            gap: "1rem",
            paddingBottom: "100px", // Bottom padding
          }}
        >
          <DarkInput
            placeholder="Type something..."
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            style={{ width: "100%" }} // Full width of column
          />
          <DarkButton text="Submit" onClick={handleClick} />
        </div>
      </div>
    </div>
  );
};

export default App
