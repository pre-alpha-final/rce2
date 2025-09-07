import { InputHTMLAttributes } from "react";
import "./CustomInput.css";

interface CustomInputProps extends InputHTMLAttributes<HTMLInputElement> {
  placeholder?: string;
}

export default function CustomInput(props: CustomInputProps) {
  return <input className="custom-input" {...props} />;
}
