<template>
    <div class="LoadingContainer">
        <div class="loading">
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        <div></div>
        </div>
    </div>

</template>


<style scoped>
.LoadingContainer{
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    width:100%;
    height:100%;
}
.loading-text{
    font-size:17px;
    font-weight: bold;
    margin-top:10px;
    margin-bottom: 20px;
}
.loading,
.loading > div {
  position: relative;
  box-sizing: border-box;
}

.loading {
  display: block;
  font-size: 0;
  color: rgba(53, 92, 125, 1);
}

.loading.la-dark {
  color: rgba(53, 92, 125, 0.7);
}

.loading > div {
  display: inline-block;
  float: none;
  background-color: currentColor;
  border: 0 solid currentColor;
}

.loading {
  width: 32px;
  height: 32px;
}

.loading > div {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 8px;
  height: 8px;
  margin-top: -4px;
  margin-left: -4px;
  border-radius: 100%;
  animation: ball-spin-clockwise-fade 1s infinite linear;
}

.loading > div:nth-child(1) {
  top: 5%;
  left: 50%;
  animation-delay: -0.875s;
}

.loading > div:nth-child(2) {
  top: 18.1801948466%;
  left: 81.8198051534%;
  animation-delay: -0.75s;
}

.loading > div:nth-child(3) {
  top: 50%;
  left: 95%;
  animation-delay: -0.625s;
}

.loading > div:nth-child(4) {
  top: 81.8198051534%;
  left: 81.8198051534%;
  animation-delay: -0.5s;
}

.loading > div:nth-child(5) {
  top: 94.9999999966%;
  left: 50.0000000005%;
  animation-delay: -0.375s;
}

.loading > div:nth-child(6) {
  top: 81.8198046966%;
  left: 18.1801949248%;
  animation-delay: -0.25s;
}

.loading > div:nth-child(7) {
  top: 49.9999750815%;
  left: 5.0000051215%;
  animation-delay: -0.125s;
}

.loading > div:nth-child(8) {
  top: 18.179464974%;
  left: 18.1803700518%;
  animation-delay: 0s;
}

.loading.la-sm {
  width: 16px;
  height: 16px;
}

.loading.la-sm > div {
  width: 4px;
  height: 4px;
  margin-top: -2px;
  margin-left: -2px;
}

.loading.la-2x {
  width: 64px;
  height: 64px;
}

.loading.la-2x > div {
  width: 16px;
  height: 16px;
  margin-top: -8px;
  margin-left: -8px;
}

.loading.la-3x {
  width: 96px;
  height: 96px;
}

.loading.la-3x > div {
  width: 24px;
  height: 24px;
  margin-top: -12px;
  margin-left: -12px;
}

@keyframes ball-spin-clockwise-fade {
  50% {
    opacity: 0.25;
    transform: scale(0.5);
  }

  100% {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
