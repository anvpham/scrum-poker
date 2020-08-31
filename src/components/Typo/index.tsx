import React, { Component } from 'react';

interface Props {
  type?: keyof JSX.IntrinsicElements;
  className?: keyof JSX.IntrinsicElements;
}

const Typo: React.FC<Props> = ({ type, children, className }) => {
  const Component = type || 'p';
  return <Component className={className}>{children}</Component>;
};

export default Typo;
