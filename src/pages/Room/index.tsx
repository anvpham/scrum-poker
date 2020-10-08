import React, { useState, useEffect, useContext } from 'react';
import Header from './Header';
import Footer from './Footer';
import style from './style.module.scss';

const Room: React.FC = () => {
  return (
    <div className={style.pokingRoom}>
      <Header />
      <Footer />
    </div>
  );
};

export default Room;
