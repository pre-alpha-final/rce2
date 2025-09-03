import React, { InputHTMLAttributes } from "react";
import "./DarkInput.css";

interface DarkInputProps extends InputHTMLAttributes<HTMLInputElement> {
  placeholder?: string;
}

const DarkInput: React.FC<DarkInputProps> = (props) => {
  return <input className="dark-input" {...props} />;
};

export default DarkInput;