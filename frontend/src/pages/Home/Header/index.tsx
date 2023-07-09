import { Avatar, Typo, Button } from '@scrpoker/components';
import style from './style.module.scss';
import React from 'react';
import { connect } from 'react-redux';
import { useHistory } from 'react-router';

interface Props {
  userName: string;
}

const Header: React.FC<Props> = ({ userName }) => {

  const history = useHistory();

  return (
    <div className={style.header}>
      <Avatar letter={userName[0]} className={style.avatar} />
      <div className={style.greeting}>
        <Typo type="h2">Hi {userName}, have a nice day!</Typo>
        <Button onClick={() => {
          const allCookies = document.cookie.split(';');
            
          for (let i = 0; i < allCookies.length; i++) {
              document.cookie = allCookies[i] + '=;expires=' + new Date(0).toUTCString();
          }

          window.location.replace('/login');
        }} icon="sign-out-alt" className={style.signOut}>Sign out</Button>
      </div>
    </div>
  );
};

const mapStateToProps = ({ userData: { name } }: IGlobalState) => {
  return {
    userName: name,
  };
};

export default connect(mapStateToProps)(Header);
