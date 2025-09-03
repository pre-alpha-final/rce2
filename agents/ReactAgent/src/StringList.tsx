import React from "react";
import "./StringList.css";

interface StringListProps {
  items: string[];
  title?: string;
}

const StringList: React.FC<StringListProps> = ({ items, title }) => {
  return (
    <div className="string-list-container">
      {title && <h2 className="string-list-title">{title}</h2>}
      <ul className="string-list">
        {items.map((item, index) => (
          <li key={index} className="string-list-item">
            {item}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default StringList;