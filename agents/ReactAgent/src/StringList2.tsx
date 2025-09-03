import React from "react";
import "./StringList2.css";

interface StringList2Props {
  items: string[];
  title?: string;
}

const colors = [
  "#FF6B6B",
  "#FFD93D",
  "#6BCB77",
  "#4D96FF",
  "#845EC2",
  "#FF9671",
];

const StringList2: React.FC<StringList2Props> = ({ items, title }) => {
  return (
    <div className="string-list2-container">
      {title && <h2 className="string-list2-title">{title}</h2>}
      <div className="string-list2">
        {items.map((item, index) => {
          const color = colors[index % colors.length];
          return (
            <span
              key={index}
              className="string-list2-item"
              style={{ backgroundColor: color }}
            >
              {item}
            </span>
          );
        })}
      </div>
    </div>
  );
};

export default StringList2;